using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAME_ARCADE
{
    class Program
    {
        
        #region Andreas
        //there cannot be a tile with 0 bombs besides it, and therefor 9 has been assigned as bombs
        enum Tile { Unmarked = 0, Marked_1 = 1, Marked_2 = 2, Marked_3 = 3, Marked_4 = 4, Marked_5 = 5, Marked_6 = 6, Marked_7 = 7, Marked_8 = 8, Marked_Bomb = 9 }
        const char BorderMarker = '█', FoWMarker = '░';

        static Tile[,] playingField;
        static bool?[,] visibleField;



        /// <summary>
        /// MineSweeper made by Andreas!
        /// </summary>
        static void GameA()
        {
            Console.WindowWidth = Console.LargestWindowWidth;
            Console.WindowHeight = Console.LargestWindowHeight;

            Random ran;
            string temp;
            bool tempBool;
            int bombsFound = 0;
            int dimX, dimY, seed, bombAmount;

            //Continues looping until the used writes a valid integer for use, OR leaves the text area blank.
            //If left blank, Seeds the random with the Datetime in Milliseconds instead.
            Console.WriteLine("Please select a seed to use, if you don't write anything, Datetime in milliseconds will be used instead");
            while (!int.TryParse(temp = PromptUser("Please select a seed to use, if you don't write anything, Datetime in milliseconds will be used instead"), out seed) || temp == "") ;
            if (temp == "") seed = DateTime.Now.Millisecond;

            //Instantiates the Random with the seed.
            ran = new Random(seed);

            //Asks the user how big he'd like the playing field to be.
            //Once again loops until the user gives good inputs. this may be a neverending loop, depending on who uses it.
            do
            {
                while (!(temp = PromptUser("What dimensions would you like your board to be? Please seperate your values by 'X,Y' without any space inbetween")).Contains(',')) ;
            } while (!ExtractDimensions(temp, out dimX, out dimY));

            //Asks the user how many bombs he'd like to use in the game.
            //Loops eternally if the user keeps giving bad requests.
            while (!int.TryParse(PromptUser("How many bombs would you like in your game? the more you get, the harder it'll be!"), out bombAmount)) ;

            //Summons the playing field.
            playingField = CreateField(dimX, dimY, ref bombAmount, ran);
            DrawField(dimX, dimY);

            while (tempBool = Play(dimX, dimY, bombAmount, ref bombsFound, ran))
            {
                if (bombsFound == bombAmount)
                {
                    playingField = CreateField(dimX, dimY, ref bombAmount, ran);
                    DrawField(dimX, dimY);
                }
            }
        }

        /// <summary>
        /// Handles the win-screen.
        /// </summary>
        /// <param name="bombAmount">The amount of bombs the player just averted</param>
        /// <returns>The players desire to keep playing</returns>
        static bool Win(int bombAmount)
        {
            //Congratulates the player on winning, and writes the amount of bombs he successfully averted.
            Console.Clear();
            Console.WriteLine("Congratulations, you won! you found a total of {0} bombs", bombAmount);
            Console.WriteLine("Would you like to play again? (Y/N)");

            //Loops until the player gives the answer to wether he wants to play again or not.
            //Makes the string lowercase, to ignore the difference between 'y' and 'Y'
            while (true)
            {
                switch (Console.ReadLine().ToLower())
                {
                    case "y": return true;
                    case "n": return false;
                }
            }
        }

        /// <summary>
        /// Handles the playing of the game.
        /// </summary>
        /// <param name="playingField">The playing field to be used.</param>
        /// <param name="visibleField">The overlaying visibility field of the playing field.</param>
        /// <param name="dimX">the length in the xth dimension</param>
        /// <param name="dimY">the length in the yth dimension</param>
        /// <param name="bombAmount">The amount of bombs total in the playing area.</param>
        /// <param name="bombsFound">The amount of bombs found during the session</param>
        /// <returns>Returns wether or not the player wishes to continue playing</returns>
        static bool Play(int dimX, int dimY, int bombAmount, ref int bombsFound, Random ran)
        {
            bool keepPlaying = true;
            string command = "";
            bool bombFound;

            //Handles the user input, and converts it to usable values.
            do
            {
                while (!((command = PromptUser("There are two types of command, Mark {x:y} and Select {x:y}, or you could Quit", 0, dimY + 6)).Contains(':') && command.Contains(' ')) || command.ToLower() == "quit") ;
            } while (!ProcessCommand(dimX, dimY, bombAmount - bombsFound, command, out keepPlaying, out bombFound, ran));

            if (bombFound) bombsFound++;

            if (bombsFound == bombAmount)
            {
                keepPlaying = Win(bombAmount);
            }

            //Redraws the entire field as it is after the move.
            DrawField(dimX, dimY);

            return keepPlaying;
        }

        /// <summary>
        /// Handles everything about dying.
        /// </summary>
        /// <returns>Returns wether or not the player wants to continue playing.</returns>
        static bool Dead()
        {
            Console.Clear();
            Console.WriteLine("Oh no! you died!");
            Console.WriteLine("Would you like to play again? (Y/N)");

            while (true)
            {
                switch (Console.ReadLine().ToLower())
                {
                    case "y": return true;
                    case "n": return false;
                }
            }
        }

        /// <summary>
        /// Processes the player command, and carries it out.
        /// </summary>
        /// <param name="playingField">The Playing field</param>
        /// <param name="dimX">the length of the xth dimension of the playing field</param>
        /// <param name="dimY">the length of the yth dimension of the playing field</param>
        /// <param name="bombsLeft">the amount of bombs left in the game</param>
        /// <param name="command">the raw command given by the player</param>
        /// <param name="keepPlaying">wether or not the player wishes to continue playing</param>
        /// <param name="bombFound">wether or not the selected tile was a bomb.</param>
        /// <returns>wether or not the command was processable.</returns>
        static bool ProcessCommand(int dimX, int dimY, int bombsLeft, string command, out bool keepPlaying, out bool bombFound, Random ran)
        {
            string[] commandSplit = command.ToLower().Split(' ', ':');
            int tempX, tempY;

            switch (commandSplit[0])
            {
                case "mark":
                    keepPlaying = true;
                    try
                    {
                        visibleField[tempX = int.Parse(commandSplit[1]), tempY = int.Parse(commandSplit[2])] = null;
                        bombFound = playingField[tempX, tempY] == Tile.Marked_Bomb;
                        return true;
                    }
                    catch
                    {
                        bombFound = false;
                        return false;
                    }
                case "select":
                    keepPlaying = true;
                    try
                    {
                        if (bombFound = playingField[tempX = int.Parse(commandSplit[1]), tempY = int.Parse(commandSplit[2])] == Tile.Marked_Bomb)
                        {
                            keepPlaying = Dead();
                            if (keepPlaying)
                            {
                                CreateField(dimX, dimY, ref bombsLeft, ran);
                            }
                        }
                        else if (playingField[tempX, tempY] == Tile.Unmarked) Reveal(tempX, tempY);
                        else visibleField[tempX, tempY] = true;

                        return true;
                    }
                    catch
                    {
                        bombFound = false;
                        return false;
                    }
                case "quit":
                    keepPlaying = false;
                    bombFound = false;
                    return true;
                default:
                    keepPlaying = true;
                    bombFound = false;
                    return false;
            }
        }

        /// <summary>
        /// Overload of PromptUser(string, bool)
        /// </summary>
        /// <param name="message">The message to write for the user</param>
        /// <param name="screenPosX">The desired screen position's x value</param>
        /// <param name="screenPosY">The desired screen position's y value</param>
        /// <returns>The player's message</returns>
        static string PromptUser(string message, int screenPosX, int screenPosY)
        {
            Console.SetCursorPosition(screenPosX, screenPosY);
            return PromptUser(message, false);
        }

        /// <summary>
        /// The Original PromptUser, prompts the user for an answer.
        /// </summary>
        /// <param name="message">The message detailing what the prompt desires specifically</param>
        /// <param name="clearScreen">Wether or not the screen should be cleared to display this prompt</param>
        /// <returns>The player's input.</returns>
        static string PromptUser(string message, bool clearScreen)
        {
            //May clear the screen, and writes the used a message. Then returns his input.
            if (clearScreen) Console.Clear();
            Console.WriteLine(message);
            return Console.ReadLine();
        }

        /// <summary>
        /// Overload of PromptUser(string, bool)
        /// </summary>
        /// <param name="message">The desired message to write for the user</param>
        /// <returns>The users input.</returns>
        static string PromptUser(string message)
        {
            //Overload of PromptUser, for convenience.
            return PromptUser(message, false);
        }

        /// <summary>
        /// Extracts the x and y coordinate out of the users input.
        /// </summary>
        /// <param name="userInput">The raw user input.</param>
        /// <param name="x">The desired X coordinate</param>
        /// <param name="y">The desired Y Coordinate</param>
        /// <returns>If the dimensions was extractable.</returns>
        static bool ExtractDimensions(string userInput, out int x, out int y)
        {
            //Splits the string on the ',' character.
            string[] temp = userInput.Split(',');

            //Attempts assigning x and y their values, returns true if this succeeds.
            try
            {
                x = int.Parse(temp[0]);
                y = int.Parse(temp[1]);
                return true;
            }
            //If an error appears due to bad info given by the user, it goes here, and puts x and y to 0, then returns false.
            catch
            {
                x = y = 0;
                return false;
            }
        }

        /// <summary>
        /// Draws the playing area in the console.
        /// </summary>
        /// <param name="playingField">The playing field to draw</param>
        /// <param name="visibleField">The visibility field to take into account when drawing</param>
        /// <param name="xDimension">the length of the games xth dimension</param>
        /// <param name="yDimension">the length of the games yth dimension</param>
        static void DrawField(int xDimension, int yDimension)
        {
            Console.Clear();

            for (int x = 0; x < xDimension + 3; x++)
            {
                for (int y = 0; y < yDimension + 3; y++)
                {
                    Console.SetCursorPosition(x * 2, y);
                    if (x == 0 && y > 1) Console.Write(y - 2);
                    else if (y == 0 && x > 1) Console.Write(x - 2);
                    else if (x == 0 || y == 0) continue;
                    else if (x == 1 || x == xDimension + 2 || y == 1 || y == yDimension + 2)
                    {
                        //Draws a nice border around the playing field.
                        Console.Write(BorderMarker + "" + BorderMarker);
                    }
                    else
                    {
                        //If the character is visible yet, it'll be drawn, otherwise the Fog of War character will be drawn instead.
                        char c = ConvertToWritableChar(playingField[x - 2, y - 2]);
                        if (visibleField[x - 2, y - 2] == null) Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(visibleField[x - 2, y - 2] == null ? 'X' : visibleField[x - 2, y - 2].Value ? ConvertToWritableChar(playingField[x - 2, y - 2]) : FoWMarker);

                        //Console.Write(ConvertToWritableChar(field[x - 2, y - 2]));
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                }
            }
        }

        /// <summary>
        /// Converts the Tile value to a writable char value for display
        /// </summary>
        /// <param name="tile">The tile to convert</param>
        /// <returns>the char to display</returns>
        static char ConvertToWritableChar(Tile tile)
        {
            //Converts the tile enum to a writable char, that can be directly used on the console.
            switch ((int)tile)
            {
                case 0:
                    return ' ';
                case 9:
                    return 'Ó';
                case 10:
                    return 'X';
                default:
                    return (char)(tile + 48);
            }
        }

        /// <summary>
        /// Reveals all the empty neighbouring tiles.
        /// </summary>
        /// <param name="x">the x-value of the coordinate to reveal</param>
        /// <param name="y">the y-value of the coordinate to reveal</param>
        /// <param name="visibleField">the current visibility layer of the game</param>
        /// <param name="playingField">the playing area</param>
        static void Reveal(int x, int y)
        {
            //Recursive function. calls itself until every neighbouring empty tile is displayed.

            //Try/Catch, in case it attempts accessing outside of the playingfield.
            try
            {
                if (visibleField[x, y] == null || visibleField[x, y] == true) return;
                else if (playingField[x, y] == Tile.Unmarked)
                {
                    visibleField[x, y] = true;

                    Console.SetCursorPosition(x + 3, y + 2);
                    Console.Write(ConvertToWritableChar(playingField[x, y]));

                    for (int x1 = x - 1; x1 <= x + 1; x1++)
                    {
                        for (int y1 = y - 1; y1 <= y + 1; y1++)
                        {
                            Reveal(x1, y1);
                        }
                    }
                }
                else
                {
                    visibleField[x, y] = true;
                    Console.SetCursorPosition(x + 3, y + 2);
                    Console.Write(ConvertToWritableChar(playingField[x, y]));
                }
            }
            //Just returns in case of errors, most likely caused by trying to access outside the playing field.
            //May be caused by Stack Overflow exception too, in case of larger playing fields.
            catch { return; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xDimension"></param>
        /// <param name="yDimension"></param>
        /// <param name="bombAmount"></param>
        /// <param name="ran"></param>
        /// <param name="visibleField"></param>
        /// <returns></returns>
        static Tile[,] CreateField(int xDimension, int yDimension, ref int bombAmount, Random ran)
        {
            Tile[,] temp = new Tile[xDimension, yDimension];
            bool?[,] boolField = new bool?[xDimension, yDimension];
            int tempX, tempY, actualBombAmount = 0;
            Tile tempTile;

            //Adds empty tiles all over the playing field.
            for (int x = 0; x < xDimension; x++)
            {
                for (int y = 0; y < yDimension; y++)
                {
                    temp[x, y] = 0;
                    boolField[x, y] = false;
                }
            }

            //Places the bomb, and plusses every surrounding tile with 1, UNLESS the tile is a bomb already.
            //If the randomly selected tile happens to be a bomb, the loop simply continues.
            //This means if the player set his playing field to 1x1, and 3000 bombs, he will only encounter one.
            //A 10x10 field with 100 bombs may not have all 100 bombs included.
            for (int x = 0; x < bombAmount; x++)
            {
                tempTile = temp[tempX = ran.Next(0, xDimension), tempY = ran.Next(0, yDimension)];
                if (tempTile == Tile.Marked_Bomb) continue;
                temp[tempX, tempY] = Tile.Marked_Bomb;
                actualBombAmount++;

                //plusses every surrounding tile with 1, unless the tile is a bomb. the tile will never exceed 8, as it can never have more than 8 neighbouring tiles.
                for (int i = tempX == 0 ? 0 : tempX - 1; (tempX == xDimension - 1) ? i <= tempX : i <= tempX + 1; i++)
                {
                    for (int j = tempY == 0 ? 0 : tempY - 1; (tempY == yDimension - 1) ? j <= tempY : j <= tempY + 1; j++)
                    {
                        temp[i, j] = temp[i, j] == Tile.Marked_Bomb ? Tile.Marked_Bomb : temp[i, j] + 1;
                    }
                }
            }

            // Laver den bomb-amount som spillet tjekker op imod, lig det antal bomber der faktisk er, i stedet for det antal ønskede bomber spilleren bad om.
            bombAmount = actualBombAmount;
            visibleField = boolField;
            return temp;
        }
        #endregion

        //gg

        #region Dennis

        enum board { film, sport, actors, random, anime, games, Hundred = 100, TwoHundred = 200, ThreeHundred = 300, FourHundred = 400, FiveHundred = 500, SixHundred = 600 } // creating dataypes




        static int point = 0;
        static string category(string Space, int TotalLength) // creating a string that can hold spaces
        {

            while (Space.Length < TotalLength) Space += " "; // makes spaces
            return Space;

        }

        static void SpilD()
        {
            Console.BackgroundColor = ConsoleColor.Blue; // makes the background color to blue
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow; // makes the text to yellow
            Console.WriteLine("                                      ###############################");
            Console.WriteLine("                                      #    Welcome to Jeopardy!     #");//Welcome screen
            Console.WriteLine("                                      ###############################");
            Console.WriteLine("Hello! Please enter your name."); // enter your name
            string name = Console.ReadLine();   // Reads the user's input
            Console.WriteLine("Welcome {0}.", name); // Prints the welcome screen with your name

            board[,] pBoard = new board[6, 6]; // decides how the board looks like
            for (byte x = 0; x < pBoard.GetLength(1); x++) // Creates the board length
            {
                switch (x % 6) // creates the board with the different categories
                {
                    case 0:
                        pBoard[0, x] = board.film;
                        break;
                    case 1:
                        pBoard[0, x] = board.sport;
                        break;
                    case 2:
                        pBoard[0, x] = board.actors;
                        break;
                    case 3:
                        pBoard[0, x] = board.random;
                        break;
                    case 4:
                        pBoard[0, x] = board.anime;
                        break;
                    case 5:
                        pBoard[0, x] = board.games;
                        break;
                }
                pBoard[1, x] = board.Hundred;
                pBoard[2, x] = board.TwoHundred;
                pBoard[3, x] = board.ThreeHundred;
                pBoard[4, x] = board.FourHundred;
                pBoard[5, x] = board.FiveHundred;
            }
            for (int i = 0; i < 5; i++) // making the player only have 5 choices to chose before it ends the game
            {

                for (int x = 0; x < pBoard.GetLength(0); x++) // Making the boards length for x
                {
                    for (int y = 0; y < pBoard.GetLength(0); y++) // making the boards length for y
                    {
                        if ((int)pBoard[x, y] < 100) // using if else statements to make spaces for the category
                        {
                            Console.Write(category(pBoard[x, y].ToString(), 12)); // creates the board to make space length to 12 for each categories
                        }
                        else
                        {
                            Console.Write(category(((int)pBoard[x, y]).ToString(), 12)); // making the space length 12 for the categories
                        }

                    }
                    Console.WriteLine("\n");

                }
                string answer, ans = "none"; // Create a string to hold a variable

                Console.WriteLine("Please type a catagory you want");
                string NewCategory = Console.ReadLine();
                Console.WriteLine("You chose {0}", NewCategory); // makes the player choose a category

                Console.WriteLine("Now choose how many points");  // makes the player choose how many points he wants to play for
                int points = Convert.ToInt32(Console.ReadLine());
                Console.WriteLine("You chose catagory {0} to {1} press any key to continue", NewCategory, points); // prints out which category and points he chose
                while (ans == "none")
                {
                    // Category = Console.ReadLine();
                    Console.Clear();
                    switch (NewCategory) //searches for the category
                    {
                        case "film":
                            switch (points) // giving points out to each category in film
                            {
                                case 100:
                                    Console.WriteLine("Which film did Leonardo Di Caprio get his Oscar from?");
                                    ans = "The Revenant"; // Answer to 100 points
                                    break;
                                case 200:
                                    Console.WriteLine("Name the film of the blue characters");
                                    ans = "Avatar"; // Answer to 200 points
                                    break;
                                case 300:
                                    Console.WriteLine("Which award did Jackie Chan get?");
                                    ans = "Acadamy award"; // Answer to 300 points
                                    break;
                                case 400:
                                    Console.WriteLine("Whhat famous movie was Robert Downey Jr in?");
                                    ans = "Iron man"; // Answer to 400 points
                                    break;
                                case 500:
                                    Console.WriteLine("What famous role did both Cary Grant and Noel Coward reject?");
                                    ans = "James Bond"; // Answer to 500 points
                                    break;
                            }
                            break;

                        case "sport":
                            switch (points) // giving points out to each category in sport
                            {
                                case 100:
                                    Console.WriteLine("Enter the full name of the person who headbutted Matererazzi in the Final World Cup");
                                    ans = "Zinedine Zidane";
                                    break;
                                case 200:
                                    Console.WriteLine("Which team is Christiano Ronaldo playing for?");
                                    ans = "Real Madrid";
                                    break;
                                case 300:
                                    Console.WriteLine("Which team is Messi playing for?");
                                    ans = "Barcalona";
                                    break;
                                case 400:
                                    Console.WriteLine("What was the score when Germany played against Brazil in World Cup 2014");
                                    ans = "7-1";
                                    break;
                                case 500:
                                    Console.WriteLine("Which team beat Iceland in the Europe cup 2016?");
                                    ans = "France";
                                    break;
                            }
                            break;

                        case "actors":
                            switch (points) // giving points out to each category in actors
                            {
                                case 100:
                                    Console.WriteLine("Enter the full name of the main actor who was in the film Yes Man");
                                    ans = "Jim Carrey";
                                    break;
                                case 200:
                                    Console.WriteLine("Enter the super hero who got cured from caner");
                                    ans = "Deadpool";
                                    break;
                                case 300:
                                    Console.WriteLine("What is the character name of the movie I Am Legend");
                                    ans = "Will Smith";
                                    break;
                                case 400:
                                    Console.WriteLine("Who is the ritchest actor?");
                                    ans = "Robert Downey Jr";
                                    break;
                                case 500:
                                    Console.WriteLine("Who was the male actor that played in Titanic?");
                                    ans = "Leonardo Di Caprio";
                                    break;
                            }
                            break;

                        case "random":
                            switch (points) // giving points out to each category in film
                            {
                                case 100:
                                    Console.WriteLine("When a male penguin falls in love what does he give to the female penguin?");
                                    ans = "Pebble";
                                    break;
                                case 200:
                                    Console.WriteLine("Who is the study secretary of Dania Grenaa?");
                                    ans = "Sheena";
                                    break;
                                case 300:
                                    Console.WriteLine("What do you call an apple that is bit on?");
                                    ans = "Apple";
                                    break;
                                case 400:
                                    Console.WriteLine("Who did Johnny Depp play in Pirates of the Carribean?");
                                    ans = "Jack Sparrow";
                                    break;
                                case 500:
                                    Console.WriteLine("What country in the world has the most tornadoes?");
                                    ans = "United Kingdom";
                                    break;
                            }
                            break;
                        case "anime":
                            switch (points) // giving points out to each category in anime
                            {
                                case 100:
                                    Console.WriteLine("How many dragon balls are there in total?");
                                    ans = "7";
                                    break;
                                case 200:
                                    Console.WriteLine("What was the first original name of the anime Pokemon?");
                                    ans = "Pocket Monster";
                                    break;
                                case 300:
                                    Console.WriteLine("Who was Ash's first pokemon?");
                                    ans = "Pikachu";
                                    break;
                                case 400:
                                    Console.WriteLine("What is the name of the bad guys in pokemon?");
                                    ans = "Team Rocket";
                                    break;
                                case 500:
                                    Console.WriteLine("What is the name of Goku's son?");
                                    ans = "Gohan";
                                    break;
                            }
                            break;
                        case "games":
                            switch (points) // giving points out to each category in games
                            {
                                case 100:
                                    Console.WriteLine("How many Counter Strike games have been made?");
                                    ans = "5";
                                    break;
                                case 200:
                                    Console.WriteLine("What is the card game Blizzard made?");
                                    ans = "Hearthstone";
                                    break;
                                case 300:
                                    Console.WriteLine("What is the most selling console?");
                                    ans = "Wii";
                                    break;
                                case 400:
                                    Console.WriteLine("What is the most succesful mmo game?");
                                    ans = "World of Warcraft";
                                    break;
                                case 500:
                                    Console.WriteLine("What does 4x stand for?");
                                    ans = "Explore, Expand, Exploit, Exterminate";
                                    break;
                            }
                            break;
                        default:
                            Console.WriteLine("Catagory not found, please type again");
                            break;
                    }
                }
                Console.WriteLine("Please type the answer with letters or else type with numbers");
                answer = Console.ReadLine();
                if (answer.ToLower() != ans.ToLower()) // makes the letters small
                {
                    Console.Clear();
                    Console.WriteLine("Incorrect!");
                    point -= points; // subtracting points if your wrong
                    answer = Console.ReadLine();
                    Console.Clear();
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("Correct!");
                    point += points; // adding points
                    Console.ReadKey();
                    Console.Clear();
                }
            }
            Console.WriteLine("Thanks for playing {0} you got {1}!", name, point); // prints the total point you got and your name
            Console.ReadKey();
        }





        #endregion

        //gg2

        #region Seb
        /// <summary>
        /// Vi starter med at lave en enum til vores brikker, så de bliver til vores egen datatype. Det samme gør vi også med den ordering, som brikkerne skal flyttes på.
        /// </summary>
        enum Piece { Non, BKing, BQueen, BRook, BBishop, BKnight, BPawn, WKing, WQueen, WRook, WBishop, WKnight, WPawn }
        enum Order { NumAndLet }

    
    
        static void SebSpil()
        {
            //En velkomst besked, hvor der kan vælges sprog.
            Console.WriteLine("Hi, and welcome to this competetive chess game! Please select a language! 1. for english, 2. for danish.");
            Console.WriteLine("Hej, og velkommen til et spil skak! Vælg dit sprog! 1. for english, 2. for dansk.");

            int sprog = Convert.ToInt32(Console.ReadLine());

            //Brugerens input bliver gemt og convertet til en int variable. Derefter dannes der en "if else" funktion, så vi nu har en menu i to forskellige sprog.

            if (sprog == 1)

            {
                //Vi starter med at lave en boolean, så vi kan holde spillet kørende så længe vi vil.

                bool GameRun = true;
                Console.BackgroundColor = ConsoleColor.Red;

                //Nu skal vi have lavet et UI, så spillerne har overblik over, hvilke valg de har. Denne skal vi huske, skal blive der.
                //Da det bliver lavet ud fra vores brikker, sætter vi Piece til at være vores variable. Herefter danner vi et multi dimensionelt array med navn, og størrelse
                Piece[,] Gmenu = GameMenu();



                //Vi laver nu en "while" funktion, som kører loppet for spillet.
                while (GameRun)
                {
                    Console.Clear();
                    //Efter at have fjernet det tidligere tekst, går vi nu i gang med at sætte UI op. Dette gøres med simpelt Console.WriteLine med forskellige muligheder.
                    ChoiceMenu(Gmenu);
                    Console.WriteLine("Goal is to kill the enemy king! Pick an action!");
                    Console.WriteLine("Move = Move a piece.");
                    Console.WriteLine("Reset = Begins a new game.");
                    Console.WriteLine("Exit = Closes the game.");

                    //Da vi nu har sat de forskellige valgmuligheder op, laver vi en "switch" funktion, som binder de forskellige valg, til de forskellige steder i koden.

                    switch (Console.ReadLine())
                    {
                        case "Move":
                            Move(Gmenu);
                            break;
                        case "Reset":
                            Gmenu = GameMenu();
                            break;
                        case "Exit":
                            GameRun = false;
                            break;

                        default:
                            Console.WriteLine("That is not an option. Please try again.");
                            break;

                            // En case for hver mulighed. "Default" sættes ind, så hvis spilleren indtaster noget programmet ikke genkender, bedes der om indtastning igen.
                    }

                }
            }
            //Kode blokken for det andet sprog, bliver her skrevet ind.
            else if (sprog == 2)
            {
                bool GameRun = true;
                Console.BackgroundColor = ConsoleColor.Green;
                Piece[,] Gmenu = GameMenu();
                while (GameRun)
                {
                    Console.Clear();
                    ChoiceMenu(Gmenu);
                    Console.WriteLine("Du vinder når du har slået den anden konge ihjel!");
                    Console.WriteLine("Move = Flyt en af dine brikker.");
                    Console.WriteLine("Reset = Ryd brættet og start forfra.");
                    Console.WriteLine("Exit = Luk spillet ned. Men det vil du ikke!");

                    switch (Console.ReadLine())
                    {
                        case "Move":
                            Move(Gmenu);
                            break;
                        case "Reset":
                            Gmenu = GameMenu();
                            break;
                        case "Exit":
                            GameRun = false;
                            break;

                        default:
                            Console.WriteLine("Kunne ikke genkende svaret. Prøv igen!");
                            break;


                    }

                }

            }


        }
        /// <summary>
        /// Nu har vi oprette et UI til spilleren, som kan bruges til at spille spillet. 
        /// Derfor skal vi nu have dannet koden til de forskellige valgmuligheder, så spillet kan køre.
        /// Vi starter med at sætte boardet op for menuen.
        /// </summary>
        /// <param name="board"></param>
        static void ChoiceMenu(Piece[,] board)
        {


            Console.Write("  ");
            for (byte x = 0; x < board.GetLength(1); x++)
            {
                if (Border == Order.NumAndLet)
                {
                    Console.Write(" {0}", (char)(x + 97));

                    //Her sætter vi vores x-akse op. Vi bruger her ascii-kode, for at programmet kan forstå hvad det er vi vil med bogstaverne.
                }

            }
            Console.Write("\n");

            for (byte y = 0; y < board.GetLength(0); y++)
            {
                Console.Write("{0} ", y + 1);

                for (byte x = 0; x < board.GetLength(1); x++)
                {
                    Console.Write(" ");


                    Brik(board[x, y]);

                    //Y-aksen er nu sat op, på samme måde som x-aksen.
                }

                Console.Write("\n");
            }


            //Vi er her færdige med at sætte boardet op, hvor vi blandt andet koder ind hvordan boardet skal se ud på akserne. Dette skal høre sammen med den måde vi koder, hvordan vi flytter brikkerne. 

        }
        /// <summary>
        /// Vi indtaster her en static, som sætter den måde koordinaterne for brikkernes bevægelser, skal tastes ind fra brugeren.
        /// </summary>
        static Order Border = Order.NumAndLet;

        //Vi sætter nu vores brikker på plads på vores board, på de koordinater de skal sidde på.

        static Piece[,] GameMenu()
        {
            //Dette vi skriver op her, er hvordan vores brikker skal placeres på vores bræt.

            Piece[,] ChoiceMenu = new Piece[8, 8];

            //Da "pawns" fra begge sider, fylder en hel linje på hver side, kan vi nemt skrive dem ind sådan her. 
            for (byte x = 0; x < ChoiceMenu.GetLength(1); x++)
            {
                ChoiceMenu[x, 1] = Piece.WPawn;
                ChoiceMenu[x, 6] = Piece.BPawn;
                //Her sætter vi altså programmet til at sætte de hvide pawns langs række 1 som egentlig er 2. Vi skal nemlig huske arrays er 0-indekseret.
                //Det samme sker selvfølgelig også for de sorte pawns, bare ved række 6, eller række 7 som det ses i spillet.
            }
            //Når det kommer til at andre brikker, som ikke skal fylde en hel linje, skal hver koordinat skrives ind seperat.
            ChoiceMenu[0, 0] = Piece.WRook;
            ChoiceMenu[1, 0] = Piece.WKnight;
            ChoiceMenu[2, 0] = Piece.WBishop;
            ChoiceMenu[3, 0] = Piece.WQueen;
            ChoiceMenu[4, 0] = Piece.WKing;
            ChoiceMenu[5, 0] = Piece.WBishop;
            ChoiceMenu[6, 0] = Piece.WKnight;
            ChoiceMenu[7, 0] = Piece.WRook;



            ChoiceMenu[0, 7] = Piece.BRook;
            ChoiceMenu[1, 7] = Piece.BKnight;
            ChoiceMenu[2, 7] = Piece.BBishop;
            ChoiceMenu[3, 7] = Piece.BQueen;
            ChoiceMenu[4, 7] = Piece.BKing;
            ChoiceMenu[5, 7] = Piece.BBishop;
            ChoiceMenu[6, 7] = Piece.BKnight;
            ChoiceMenu[7, 7] = Piece.BRook;

            return ChoiceMenu;

            //Vi har nu sat vores brikker op, på de koordinater de har når spillet starter.

        }


        /// <summary>
        /// Brikker og board er nu kodet ind. Vi fortsætter derfor til at forkorte navnene på brikkerne. 
        /// </summary>


        static void Brik(Piece ValgtBrik)

        {
            Console.ForegroundColor = (ValgtBrik < Piece.WKing) ? ConsoleColor.White : ((ValgtBrik < Piece.BKing) ? ConsoleColor.White : ConsoleColor.Black); switch (ValgtBrik)

            {
                //Min kode gav den ene spilside og de blanke pladser samme farve af en eller anden grund. Jeg har ikke kunne funde ud af hvorfor. Derfor har jeg specifikt bestemt de blankes farve, i koden forneden.
                case Piece.Non:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("#");
                    break;
                case Piece.WKing:
                case Piece.BKing:
                    Console.Write("K");
                    break;
                case Piece.WQueen:
                case Piece.BQueen:
                    Console.Write("Q");
                    break;
                case Piece.WRook:
                case Piece.BRook:
                    Console.Write("R");
                    break;
                case Piece.WBishop:
                case Piece.BBishop:
                    Console.Write("B");
                    break;
                case Piece.WPawn:
                case Piece.BPawn:
                    Console.Write("P");
                    break;
                case Piece.WKnight:
                case Piece.BKnight:
                    Console.Write("S");
                    break;

                    //Med det resultat, at alle parter nu har hver deres farve.
            }
            Console.ForegroundColor = ConsoleColor.White;

        }
        //Det eneste vi mangler nu, er sådan set at implementere bevægelse af brikkerne.
        static void Move(Piece[,] BoardMove)
        {

            Console.Clear();
            ChoiceMenu(BoardMove);
            //Jeg har valgt at tage 1-bogstav måden at flytte brikker på.
            string input = " ";
            int x1 = 0, y1 = 0, x2 = 0, y2 = 0;

            //Jeg sætter mine variabler til at være 0, for at snyde compileren. Da jeg i næste blok har en "if" sætning, kan compileren nemlig ikke være sikker på, at det der står vil være sådan hver gang jeg kører koden.
            if (Border == Order.NumAndLet)
            {


                Console.WriteLine("Please choose a piece to move. Enter coordinates like, '1b'");
                input = Console.ReadLine();
                x1 = input[1] - 97;
                y1 = input[0] - 49;

                Console.WriteLine("Please choose a coordinate, you would like you piece to move. Like '4c'");
                input = Console.ReadLine();
                x2 = input[1] - 97;
                y2 = input[0] - 49;



            }
            //Da vi nu har oprettet koden til at tage imod input for koordinater, skal vi nu execute 
            BoardMove[x2, y2] = BoardMove[x1, y1];
            BoardMove[x1, y1] = Piece.Non;

            //Koden for spillet er hermed færdig. Problemet med den måde jeg har valgt at bygge koden op på, har været at jeg ikke rigtigt kunne prøve programmet af, før koden var færdig. 
            //Men efter at få fikset nogle huller hist og her, kører spillet som det skal.

        }


    



#endregion
static void Main(string[] args)
        {
            Console.WriteLine("Hi, and welcome to GAME ARCADE! Please pick one of the available games. \n1. MineSweeper, 2. Jeopardy, 3. Chess.");

            bool looper = true;
            while (looper) 
            switch (Console.ReadLine())
            {
                case "1":
                    GameA();
                    break;
                case "2":
                    SpilD();
                    break;
                case "3":
                    SebSpil();
                    break;
                case "Quit":
                        looper = false;
                    break;
                default:
                    continue;

                   
            }


        }
    }
}

    

