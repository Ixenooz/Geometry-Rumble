using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour
{
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    private string playerName;

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        playerName = "Player " + UnityEngine.Random.Range(10, 99);
        Debug.Log("playerName: " + playerName);
        Debug.Log("-- LobbyManager started --");
    }

    private void Update()
    {
        HandleLobbyUpdates();
        HandleLobbyPollForUpdates();

        if (Input.GetKeyDown(KeyCode.C))
        {
            CreateLobby();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            ListLobbies();
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            JoinLobbyByCode("7F5TEG");
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            QuickJoinLobby();
        }
    }

    /// <summary>
    /// Handles lobby updates, such as sending heartbeat pings to keep the lobby alive.
    /// This method is called every frame to ensure the lobby remains active.
    /// If a lobby is hosted, it sends a heartbeat ping every 15 seconds.
    /// </summary>
    private async void HandleLobbyUpdates()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 15;
                heartbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    /// <summary>
    /// Handles polling for updates in the joined lobby.
    /// This method is called every frame to check for updates in the lobby state.
    /// If a lobby is joined, it checks for updates every 1.1 seconds.
    /// </summary>
    private async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                float lobbyUpdateTimerMax = 1.1f;
                lobbyUpdateTimer = lobbyUpdateTimerMax;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby; // Update joinedLobby to the latest state
            }
        }
    }

    private async void CreateLobby()
    {
        try
        {
            // Logique pour créer un lobby
            string lobbyName = "MyLobby";
            int maxPlayers = 4;

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "Arena") },
                    { "Map", new DataObject(DataObject.VisibilityOptions.Public, "Battlefield") }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            hostLobby = lobby;
            joinedLobby = hostLobby; // Set joinedLobby to the newly created lobby

            Debug.Log("Lobby created: " + lobby.Name + "; " + maxPlayers + " players" + "; ID: " + lobby.Id + "code: " + lobby.LobbyCode);

            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to create lobby: " + e);
        }
    }

    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
            Debug.Log("Lobbies found : " + queryResponse.Results.Count);

            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log("Lobby: " + lobby.Name + "; Players: " + lobby.Players.Count + "/" + lobby.MaxPlayers + "; GameMode : " + lobby.Data["GameMode"].Value);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to list lobbies: " + e);
        }
    }

    /// <summary>
    /// Attempts to join a lobby using its code.
    /// </summary>
    private async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer(),
            };
            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);

            Debug.Log("Joined lobby with code: " + lobbyCode);
            PrintPlayers(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to join lobby: " + e);
        }
    }

    /// <summary>
    /// Attempts to quickly join a lobby.
    /// </summary>
    private async void QuickJoinLobby()
    {
        try
        {

            QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions
            {
                Player = GetPlayer(),
            };

            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
            joinedLobby = lobby; // Update joinedLobby to the quick joined lobby

            Debug.Log("Quick joined lobby :" + lobby.Name);
            PrintPlayers(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to quick join lobby: " + e);
        }
    }

    /// <summary>
    /// Creates a Player object with the player's name.
    /// </summary>
    /// <returns>A Player object containing the player's data.</returns>
    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) }
            }
        };
    }

    private void PrintPlayers()
    {
        PrintPlayers(joinedLobby);
    }

    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log("List of players in lobby " + lobby.Name + ", Gamemode : " + lobby.Data["GameMode"].Value + ", Map : " + lobby.Data["Map"].Value + " :");
        foreach (Player player in lobby.Players)
        {
            Debug.Log("Player ID: " + player.Id + ", Name: " + player.Data["PlayerName"].Value);
        }
    }

    /// <summary>
    /// Updates the game mode of the hosted lobby.
    /// </summary>
    private async void UpdateLobbyGameMode(string gameMode)
    {
        try
        {
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode) }
                }
            });
            joinedLobby = hostLobby; // Update joinedLobby to the modified hostLobby

            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to update lobby game mode: " + e);
        }
    }

    /// <summary>
    /// Updates the player's name in the joined lobby.
    /// </summary>
    private async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) }
                }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to update player name: " + e);
        }
    }

    /// <summary>
    /// Leaves the currently joined lobby.
    /// </summary>
    private async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId); // Leaves the lobby using the player's ID
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to leave lobby: " + e);
        }
    }

    /// <summary>
    /// Kicks a player from the currently joined lobby.
    /// This method is intended for the host to remove another player.
    /// </summary>
    private async void KickPlayer()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[1].Id); // Kicks the second player in the lobby
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to leave lobby: " + e);
        }
    }

    private async void MirgrateLobbyHost()
    {
        try
        {
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = joinedLobby.Players[1].Id // Migrate the host to the second player in the lobby
            });
            joinedLobby = hostLobby; // Update joinedLobby to the modified hostLobby

            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to update lobby game mode: " + e);
        }
    }

    private void DeleteLobby()
    {
        try
        {
            LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
        } catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to delete lobby: " + e);
        }
    }
}
