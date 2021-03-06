﻿using System;
using System.IO;

using bit = System.Int16;

namespace HammingCode
{
    internal class Program
    {
        public static string inputTextFilePath = @"C:\Users\Lola\Desktop\HammingCode\HammingCode\HammingCode\InputMessage.txt";
        public static string encodedFilePath = @"C:\Users\Lola\Desktop\HammingCode\HammingCode\HammingCode\EncodedMessage.txt";
        public static string outputFilePath = @"C:\Users\Lola\Desktop\HammingCode\HammingCode\HammingCode\EncodedMessage.bin";

        private const int m = 8;
        private const int n = 16;

        public static bit[,] H = new bit[m, n]
        {
            {0, 1, 1, 1, 1, 0, 1, 1,  1, 0, 0, 0, 0, 0, 0, 0},
            {1, 1, 1, 1, 1, 1, 0, 0,  0, 1, 0, 0, 0, 0, 0, 0},
            {1, 0, 1, 0, 0, 0, 1, 1,  0, 0, 1, 0, 0, 0, 0, 0},
            {0, 1, 1, 0, 1, 1, 0, 1,  0, 0, 0, 1, 0, 0, 0, 0},
            {1, 0, 0, 1, 1, 0, 1, 0,  0, 0, 0, 0, 1, 0, 0, 0},
            {0, 1, 0, 1, 0, 1, 0, 1,  0, 0, 0, 0, 0, 1, 0, 0},
            {0, 0, 0, 0, 1, 1, 1, 1,  0, 0, 0, 0, 0, 0, 1, 0},
            {1, 1, 0, 0, 1, 0, 0, 1,  0, 0, 0, 0, 0, 0, 0, 1}
        };

        private static void Main(string[] args)
        {
            #region read message from file
            string messageText = File.ReadAllText(inputTextFilePath);
            FileInfo oldOutputFile = new FileInfo(outputFilePath);
            oldOutputFile.Delete();
            FileInfo outputFile = new FileInfo(outputFilePath);
            BinaryWriter bw = new BinaryWriter(outputFile.OpenWrite());

            int messageLength = messageText.Length;
            string[] encodedWords = new string[messageLength];
            #endregion

            #region encode
            // Every character's ascii representation is encoded and saved to the "Encoded.txt" file
            for (int i = 0; i < messageLength; i++)
            {
                string temp = "";
                bit[] t = Encode(messageText[i]);

                //save also binary representation
                bit letter = ConvertTwoBytesToASCII(t);
                bw.Write(letter);

                // 01110010... string representation ???
                for (int j = 0; j < n; j++)
                {
                    temp += Convert.ToString(t[j]);
                }
                encodedWords[i] = temp;
            }
            File.WriteAllLines(encodedFilePath, encodedWords);
            #endregion

            WriteLine($"You can now simulate errors by changing chosen bits in {Path.GetFileName(encodedFilePath)} file.\nWhen you finish, click any key.", ConsoleColor.Yellow);
            Console.ReadKey();
            Console.Clear();

            #region decode
            string decodedWord = "";
            string[] codeWords = File.ReadAllLines(encodedFilePath);
            bit[] intCodeWords = new bit[n];

            //Reading the saved bit sequences as Int16,
            //Then decoding each code word to ASCII
            for (int i = 0; i < messageLength; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    string temp = "";
                    temp += codeWords[i][j];
                    intCodeWords[j] = Int16.Parse(temp);
                }
                var decodedChar = Decode(intCodeWords);
                Console.WriteLine(decodedChar);
                decodedWord += decodedChar;
            }

            WriteLine("Decoded message: ", ConsoleColor.DarkCyan);
            WriteLine(decodedWord, ConsoleColor.Cyan);
            #endregion

            Console.ReadKey();
        }
        /// <summary>
        /// Writes line of text with given color (by default: White)
        /// </summary>
        private static void WriteLine(string text, ConsoleColor color = ConsoleColor.White)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = oldColor;
        }
        /// <summary>
        /// Writes text with given color (by default: White)
        /// </summary>
        private static void Write(string text, ConsoleColor color = ConsoleColor.White)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = oldColor;
        }

        /// <summary>
        /// Returns the Hamming matrix multiplied by a code vector
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static bit[] MultiplyByHamming(bit[] vector)
        {
            int[] errorTable = new int[m];
            bit[] result = new bit[m];
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    errorTable[i] += vector[j] * H[i, j];
                }
                result[i] = Convert.ToInt16(errorTable[i] % 2);
            }
            return result;
        }

        /// <summary>
        /// Encode a character to table of '0's and '1's.
        /// Uses MultiplyByHamming to encode to
        /// </summary>
        /// <param name="character">character to encode</param>
        /// <returns>table of '0's and '1's</returns>
        public static bit[] Encode(char character)
        {
            bit[] table = new bit[n];
            bit[] errorTable = new bit[m];
            int ascii = Convert.ToInt32(character);
            table = ASCIItoBinary(ascii);
            errorTable = MultiplyByHamming(table);
            for (int i = 8, j = 0; i < n; i++, j++)
            {
                table[i] = errorTable[j];
            }
            return table;
        }

        /// <summary>
        /// Decodes the binary code of single character to ASCII
        /// </summary>
        /// <param name="codedTable">table of '0's and '1's</param>
        /// <returns>Decode character</returns>
        public static char Decode(bit[] codedTable)
        {
            bit[] errorTable = MultiplyByHamming(codedTable);
            if (CheckForError(errorTable))
            {
                int x = SeekForOneError(errorTable);
                if (x != -1)
                {
                    Write("Bit on position " + x + " was distorted: ", ConsoleColor.DarkRed);
                    codedTable[x] += 1;
                    codedTable[x] %= 2;
                }
                else
                {
                    if (SeekForTwoErrors(errorTable)[0] != -1)
                    {
                        var errorIndexes = SeekForTwoErrors(errorTable);
                        Write("Bits on positions " + errorIndexes[0] + " and " + errorIndexes[1] + " were distorted: ", ConsoleColor.DarkRed);
                        codedTable[errorIndexes[0]] += 1;
                        codedTable[errorIndexes[0]] %= 2;
                        codedTable[errorIndexes[1]] += 1;
                        codedTable[errorIndexes[1]] %= 2;
                    }
                    else
                    {
                        Write("Code word is too distorted. ", ConsoleColor.Red);
                    }
                }
            }
            return Convert.ToChar(ConvertToASCII(codedTable));
        }

        /// <summary>
        /// Check if any column corresponds to the error vector.
        /// </summary>
        /// <param name="errorTable"></param>
        /// <returns> Error column's index or -1 if impossible</returns>
        public static int SeekForOneError(bit[] errorTable)
        {
            bool columnFound;
            for (int i = 0; i < n; i++)
            {
                columnFound = true;
                for (int j = 0; j < m; j++)
                {
                    if (H[j, i] != errorTable[j])
                    {
                        columnFound = false;
                        break;
                    }
                }
                if (columnFound)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Seeks for two errors columns( checks for the boolean sum of two columns)
        /// </summary>
        /// <param name="errorTable"></param>
        /// <returns> Error columns indexes or, if impossible, table with length of 2 and -1 on both positions</returns>
        public static int[] SeekForTwoErrors(bit[] errorTable)
        {
            bool columnFound;
            int[] columns = new int[2];
            for (int i = 0; i < n - 1; i++)
            {
                for (int l = i + 1; l < n; l++)
                {
                    columnFound = true;
                    for (int j = 0; j < m; j++)
                    {
                        if (((H[j, i] + H[j, l]) % 2) != errorTable[j])
                        {
                            columnFound = false;
                            break;
                        }
                    }
                    if (columnFound)
                    {
                        columns[0] = i;
                        columns[1] = l;
                        return columns;
                    }
                }
            }
            return new int[2] { -1, -1 };
        }

        /// <summary>
        /// Convert one byte, represented as e.g. 010111..., to ascii
        /// </summary>
        /// <param name="table"> Minimum length is 8</param>
        /// <returns></returns>
        public static int ConvertToASCII(bit[] table)
        {
            double code = 0.0;
            for (int i = 0, j = 7; i < m; i++, j--)
            {
                code += (table[i] * Math.Pow(2.0, j));
            }
            return (int)code;
        }

        /// <summary>
        /// Convert two bytes, represented as e.g. 010111..., to ascii
        /// </summary>
        /// <param name="table"> Minimum length is 16</param>
        /// <returns></returns>
        public static bit ConvertTwoBytesToASCII(bit[] table)
        {
            double code = 0.0;
            for (int i = 0, j = 15; i < n; i++, j--)
            {
                code += (table[i] * Math.Pow(2.0, j));
            }
            return (bit)code;
        }

        /// <summary>
        /// Convert an ASCII character to its binary (010100...) representation
        /// </summary>
        /// <param name="character">ASCII as int</param>
        /// <returns>table of '0's and '1's</returns>
        public static bit[] ASCIItoBinary(int character)
        {
            int temp;
            bit[] retTable = new bit[n];
            for (int i = 7; i != 0; i--)
            {
                temp = character % 2;
                retTable[i] = Convert.ToInt16(temp);
                character /= 2;
                if (i == 1)
                {
                    retTable[0] = 0;
                }
            }
            return retTable;
        }

        /// <summary>
        /// Check if there is any error in error table
        /// </summary>
        /// <param name="errorTable"></param>
        /// <returns></returns>
        public static bool CheckForError(bit[] errorTable)
        {
            for (int i = 0; i < m; i++)
            {
                if (errorTable[i] != 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
