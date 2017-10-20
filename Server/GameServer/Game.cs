using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameServer
{
    class Game
    {
        private byte[] gameBoard;
        private List<User> playerList;
        private List<UserInfo> scoreInfo;
        private List<int> askedQuestions;
        private Random rng = new Random();
        private ChatRoom currentRoom;
        private int numTurns = 0;
        private byte chosenTerritory = 255;

        private ChatRoomManager chatRoomManager = ChatRoomManager.Instance;
        private QuestionManager questionManager = QuestionManager.Instance;

        private bool[] playerInGame = new bool[3];
        static object locfunc = new object();

        /*
         * Gameboard outline
         *  Connections: lines; () - isolated within
         *  The numbers represent the index in gameBoard array
         * 
         *       0 - 3
         *    /  | / |
         *   1 - 4 - 6
         *    \  | \ |
         *       2 - 5(7)
         * 
         */
        
        public enum Players { Player1 = 1, Player2, Player3 }

        public Game(List<User> userList, ChatRoom room)
        {
            gameBoard = new byte[8] { 255, 255, 255, 255, 255, 255, 255, 255 };
            rng = new Random();

            for (int i = 0; i < 3; ++i)
                playerInGame[i] = true;

            //aiTurn = new AITurn();
            currentRoom = room;
            playerList = new List<User>(userList);
            scoreInfo = new List<UserInfo>();
            askedQuestions = new List<int>();
            int numPlayers = playerList.Count;

            if (playerList.Count < 3)
            {
                for (int i = 0; i < (3 - numPlayers); ++i)
                    playerList.Add(new User(null, 0, "AIPlayer" + (i + 1), null));
            }

            ShufflePlayers(playerList);

            for (int i = 0; i < 3; ++i)
            {
                scoreInfo.Add(new UserInfo(null, 0, false));
            }

            Message.SendGamePlayerListMessage(playerList);

            // wait a bit
            Thread.Sleep(300);

            numTurns = -1;
            AdvanceTurn();
        }

        public void DoAIMoveNoAttack(int playerIndex)
        {
            byte selectedTerritory = SelectRandomValidTerritory(playerIndex);
            while (gameBoard[selectedTerritory] != 255)
                selectedTerritory = SelectRandomValidTerritory(playerIndex);
            gameBoard[selectedTerritory] = (byte)playerIndex;
            UpdateScore(100);
        }

        public void UpdateScore(int delta)
        {
            UserInfo score = scoreInfo[numTurns % 3];
            score.totalScore += delta;
            scoreInfo[numTurns % 3] = score;
            Message.SendScoreUpdate(scoreInfo.ToArray(), currentRoom);
        }
        
        public bool IsTerritoryEmpty(byte move)
        {
            if (gameBoard[move] == 255)
            {
                return true;
            }
            else
                return false;
        }

        public void PerformMoveRequest(User user, byte move)
        {
            if (GetCurrentPlayer().GetSocket().Equals(user.GetSocket()))
            {
                if (numTurns < 3)
                {
                    if (IsTerritoryEmpty(move))
                    {
                        PlaceMove(numTurns % 3, move);
                        AdvanceTurn();
                    }
                    else
                    {
                        Message.SendInvalidMoveMessage(GetCurrentPlayer().GetSocket());
                        Message.SendInitialTurnMessage(GetCurrentPlayer().GetSocket());
                    }
                }
                else
                {
                    bool success = IsMoveValid(numTurns % 3, move);
                    if (success)
                    {
                        SaveMove(move);
                        SendQuestion(numTurns % 3);
                    }
                    else
                    {
                        Message.SendInvalidMoveMessage(GetCurrentPlayer().GetSocket());
                        Message.SendTurnMessage(GetCurrentPlayer().GetSocket());
                    }
                }
            }
            else
            {
                Console.WriteLine("Attempt to perform move out of turn!");
            }
        }
        
        public void AdvanceTurn()
        {
            ++numTurns;
            SendGameBoardToAll();

            if (CheckWinner())
            {
                return;
            }

            if (playerInGame[numTurns % 3] == true)
            {
                // player is not already defeated
                if (GetCurrentPlayer().GetUsername() != null)
                {
                    // player is human
                    if (numTurns < 3)
                    {
                        // we are in the preliminary stage
                        Message.SendInitialTurnMessage(GetCurrentPlayer().GetSocket());
                    }
                    else
                    {
                        if (PlayerHasAnyTerritory(numTurns % 3))
                        {
                            Message.SendTurnMessage(GetCurrentPlayer().GetSocket());
                        }
                        else
                        {
                            Message.SendGameLostMessage(GetCurrentPlayer().GetSocket());
                            playerInGame[numTurns % 3] = false;
                            if (!AnyPlayersActive())
                                return;
                            AdvanceTurn();
                        }
                    }
                }
                else
                {
                    // player is ai
                    if (numTurns < 3)
                    {
                        // we are in the preliminary stage
                        DoAIMoveNoAttack(numTurns % 3);
                        Thread.Sleep(200);
                        AdvanceTurn();
                    }
                    else
                    {
                        if (PlayerHasAnyTerritory(numTurns % 3))
                        {
                            DoAIMove(numTurns % 3);
                            Thread.Sleep(200);
                            AdvanceTurn();
                        }
                        else
                        {
                            playerInGame[numTurns % 3] = false;
                            AdvanceTurn();
                        }
                    }
                }
            }
            else
            {
                // player is defeated, skip.
                if (!AnyPlayersActive())
                    return;
                AdvanceTurn();
            }
        }


        public bool CheckWinner()
        {
            byte element = gameBoard[0];

            if (numTurns >= 3)
            {

                for (int i = 0; i < 8; ++i)
                {
                    if (gameBoard[i] != element)
                    {
                        if (gameBoard[i] == 255)
                            continue;
                        else
                        {
                            if (element == 255)
                                element = gameBoard[i];
                            else
                                return false;
                        }
                    }

                }

                if (element == 255)
                    return false;
                else
                {
                    int scoreDelta = scoreInfo[element].totalScore;
                    Message.SendGameWonMessage(playerList[element].GetSocket(), scoreDelta);
                    DBManager.Instance.ModifyScore(playerList[element].GetUsername(), scoreDelta);

                    for (int i = 0; i < 3; ++i)
                    {
                        if (i != element && playerInGame[i] && playerList[i].GetSocket() != null)
                            Message.SendGameLostMessage(playerList[i].GetSocket());
                    }
                    return true;
                }
            }
            else
                return false;
        }

        public bool AnyPlayersActive()
        {
            bool playerActive = false;
            for (int i = 0; i < 3; ++i)
            {
                if (playerList[i].GetUsername() != null && playerInGame[i] == true)
                    return true;
            }

            return playerActive;
        }

        public void ReplacePlayerWithAI(Socket socket)
        {
            for (int i = 0; i < 3; ++i)
            {
                if (playerList[i].GetSocket() != null && playerList[i].GetSocket().Equals(socket))
                {
                    playerList[i] = new User(null, scoreInfo[i].totalScore, "AIPLayer" + i);
                    Message.SendGamePlayerListMessage(playerList);
                    if (GetCurrentPlayer().GetSocket() != null && GetCurrentPlayer().GetSocket().Equals(socket))
                        AdvanceTurn();
                }
            }
        }

        public User GetCurrentPlayer()
        {
            return playerList[numTurns % 3];
        }

        public void SendGameBoardToAll()
        {
            Console.Write("Sent gameboard: ");
            for (int i = 0; i < 8; ++i)
            {
                Console.Write(gameBoard[i]);
                Console.Write(",");
            }
            Console.Write("\n");

            for (int i = 0; i < playerList.Count; ++i)
            {
                if (playerList[i].GetSocket() != null)
                    Message.SendMessage(playerList[i].GetSocket(), MessageType.GAME_BOARD_UPDATE, gameBoard);
            }
        }
        
        public void SendQuestion(int playerIndex)
        {
            QuestionInfo question = questionManager.GetRandomQuestion(ref askedQuestions);
            Message.SendGameQuestionMessage(playerList[playerIndex].GetSocket(), question);
        }

        public void SaveMove(byte move)
        {
            chosenTerritory = move;
        }

        public void ProcessQuestion(QuestionAnswer ans, User user)
        {
            User currentPlayer = GetCurrentPlayer();

            if (currentPlayer.Equals(user))
            {
                if (questionManager.CheckAnswer(ans.id, ans.option))
                {
                    PlaceMove(numTurns % 3, chosenTerritory);
                    Message.SendTurnEndMessage(currentPlayer.GetSocket());
                    //++numTurns;
                    AdvanceTurn();
                }
                else
                {
                    Message.SendGameIncorrectMessage(currentPlayer.GetSocket());
                    //++numTurns;
                    AdvanceTurn();
                }
            }
        }

        public bool DoMove(int playerIndex, byte move)
        {
            return PlaceMove(playerIndex, move);
        }

        public void DoAIMove(int playerIndex)
        {
            byte selectedTerritory = SelectRandomValidTerritory(playerIndex);
            int chance = rng.Next(1, 101);

            if (chance <= 35 || gameBoard[playerIndex] == 255)
            {
                byte previousOwner = gameBoard[selectedTerritory];
                gameBoard[selectedTerritory] = (byte)playerIndex;

                if (previousOwner == 255)
                    UpdateScore(100);
                else
                    UpdateScore(200);
                //++numTurns;
            }

        }

        //for AI
        public byte SelectRandomValidTerritory(int playerIndex)
        {
            List<int> playerTerritory = GetPlayerTerritory(playerIndex);
            Random r = new Random();
            if (!PlayerHasAnyTerritory(playerIndex) && numTurns < 3)
            {
                int randomTerritory = r.Next(0, 7);

                return (byte)randomTerritory;
            }
            else if (PlayerHasAnyTerritory(playerIndex))
            {
                List<int> validTerritories = new List<int>();
                for (int i = 0; i < playerTerritory.Count; ++i)
                {
                    switch (playerTerritory[i])
                    {
                        case 0:
                            {
                                validTerritories.Add(1);
                                validTerritories.Add(3);
                                validTerritories.Add(4);

                                break;
                            }

                        case 1:
                            {
                                validTerritories.Add(0);
                                validTerritories.Add(2);
                                validTerritories.Add(4);

                                break;
                            }
                        case 2:
                            {
                                validTerritories.Add(1);
                                validTerritories.Add(4);
                                validTerritories.Add(5);

                                break;
                            }
                        case 3:
                            {
                                validTerritories.Add(0);
                                validTerritories.Add(4);
                                validTerritories.Add(6);

                                break;
                            }
                        case 4:
                            {
                                validTerritories.Add(0);
                                validTerritories.Add(1);
                                validTerritories.Add(2);
                                validTerritories.Add(3);
                                validTerritories.Add(5);
                                validTerritories.Add(6);

                                break;
                            }
                        case 5:
                            {
                                validTerritories.Add(2);
                                validTerritories.Add(4);
                                validTerritories.Add(6);
                                validTerritories.Add(7);

                                break;
                            }
                        case 6:
                            {
                                validTerritories.Add(3);
                                validTerritories.Add(4);
                                validTerritories.Add(5);

                                break;
                            }
                        case 7:
                            {
                                validTerritories.Add(5);

                                break;
                            }
                    }
                }

                // remove duplicate valid territories
                validTerritories = validTerritories.Distinct().ToList();

                // remove owned territories
                foreach (int territory in playerTerritory)
                    validTerritories.Remove(territory);

                // reseed random generator
                r = new Random();
                int randomTerritory = r.Next(validTerritories.Count);

                Console.WriteLine("ai chose: " + validTerritories[randomTerritory]);

                return (byte)validTerritories[randomTerritory];
            }

            // we shouldn't reach this point, i think
            return 255;

        }

        public List<int> GetPlayerTerritory(int playerIndex)
        {
            List<int> playerTerritory = new List<int>();
            for (int i = 0; i < 8; ++i)
            {
                if (gameBoard[i] == playerIndex)
                    playerTerritory.Add(i);
            }

            return playerTerritory;
        }

        public ref List<int> GetAskedQuestionsList()
        {
            return ref askedQuestions;
        }

        public bool PlayerHasAnyTerritory(int playerIndex)
        {
            if (GetPlayerTerritory(playerIndex).Count > 0)
                return true;
            else
                return false;
        }

        private int GetIndexOfPlayer(Player player)
        {
            if (!player.IsAI())
                for (int i = 0; i < playerList.Count; ++i)
                {
                    if (playerList[i].GetSocket() == player.GetSocket())
                        return i;
                }
            else
                for (int i = 0; i < playerList.Count; ++i)
                {
                    if (playerList[i].GetNickname() == player.GetNickname())
                        return i;
                }

            return -1;
        }

        public byte[] GetGameBoard()
        {
            return gameBoard;
        }

        private bool PlaceMove(int playerIndex, byte move)
        {
            if (move < 0 || move > 7)
                return false;

            if (IsMoveValid(playerIndex, move))
            {
                lock (gameBoard)
                {
                    byte previousOwner = gameBoard[move];
                    gameBoard[move] = (byte)playerIndex;
                    if (previousOwner == 255)
                        UpdateScore(100);
                    else
                        UpdateScore(200);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsMoveValid(int playerIndex, byte move)
        {
            if (move < 0 || move > 7)
                return false;

            //get all territory of player
            List<int> playerTerritory = new List<int>();

            for (int i = 0; i < 8; ++i)
            {
                if (gameBoard[i] == (byte)playerIndex)
                {
                    playerTerritory.Add(i);
                }
            }

            if (playerTerritory.Count == 0 && numTurns > 3)
                return false;
            else if (playerTerritory.Count == 0 && numTurns <= 3)
            {
                return true;
            }

            //check if the move is valid
            //also check if the player clicked on his own territory
            //this should be stopped in the client, but.. better safe than sorry

            switch (move)
            {
                case 0:
                    {
                        if ((playerTerritory.Contains(1) || playerTerritory.Contains(4) || playerTerritory.Contains(3))
                            && !playerTerritory.Contains(0))
                            return true;
                        else
                            return false;
                    }
                case 1:
                    {
                        if ((playerTerritory.Contains(0) || playerTerritory.Contains(4) || playerTerritory.Contains(2))
                            && !playerTerritory.Contains(1))
                            return true;
                        else
                            return false;
                    }
                case 2:
                    {
                        if ((playerTerritory.Contains(1) || playerTerritory.Contains(4) || playerTerritory.Contains(5))
                            && !playerTerritory.Contains(2))
                            return true;
                        else
                            return false;
                    }
                case 3:
                    {
                        if ((playerTerritory.Contains(0) || playerTerritory.Contains(4) || playerTerritory.Contains(6))
                            && !playerTerritory.Contains(3))
                            return true;
                        else
                            return false;
                    }
                case 4:
                    {
                        if ((playerTerritory.Contains(0) || playerTerritory.Contains(1) || playerTerritory.Contains(2)
                            || playerTerritory.Contains(5) || playerTerritory.Contains(6) || playerTerritory.Contains(3))
                            && !playerTerritory.Contains(4))
                            return true;
                        else
                            return false;
                    }
                case 5:
                    {
                        if ((playerTerritory.Contains(4) || playerTerritory.Contains(2) || playerTerritory.Contains(6)
                            || playerTerritory.Contains(7)) && !playerTerritory.Contains(5))
                            return true;
                        else
                            return false;
                    }
                case 6:
                    {
                        if ((playerTerritory.Contains(3) || playerTerritory.Contains(4) || playerTerritory.Contains(5))
                            && !playerTerritory.Contains(6))
                            return true;
                        else
                            return false;
                    }
                case 7:
                    {
                        if ((playerTerritory.Contains(5)) && !playerTerritory.Contains(7))
                            return true;
                        else
                            return false;
                    }
            }

            //we shouldn't normally reach this point...
            return false;
        }

        public bool DoInitialMove(int playerIndex, int move)
        {
            if (move < 0 || move > 7 || gameBoard[move] != 255)
                return false;

            for (int i = 0; i < 8; ++i)
            {
                if (gameBoard[i] == (byte)playerIndex)
                    return false;
            }

            gameBoard[move] = (byte)playerIndex;

            return true;
        }

        public int GetNumTurns()
        {
            return numTurns;
        }

        private void ShufflePlayers(List<User> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                User value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

    }
}
