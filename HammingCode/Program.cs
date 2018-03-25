using System;
using System.IO;


namespace HammingCode
{
    internal class Program
    {
        private const int m = 8;
        private const int n = 16;

        public static short[,] H = new short[m, n]
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

        public static string inputTextFilePath = @"C:\Users\Lola\Desktop\HammingCode\HammingCode\HammingCode\Input.txt";
        public static string encodedFilePath = @"C:\Users\Lola\Desktop\HammingCode\HammingCode\HammingCode\Encoded.txt";
        public static string outputFilePath = @"C:\Users\Lola\Desktop\HammingCode\HammingCode\HammingCode\BinaryEncoded.bin";

        private static void Main(string[] args)
        {
            #region read message
            string messageText = File.ReadAllText(inputTextFilePath);
            FileInfo oldOutputFile = new FileInfo(outputFilePath);
            oldOutputFile.Delete();
            FileInfo outputFile = new FileInfo(outputFilePath);
            BinaryWriter bw = new BinaryWriter(outputFile.OpenWrite());

            int messageLength = messageText.Length;
            string[] encodedWords = new string[messageLength];
            #region

            #region encode
            // Every character's ascii representation is encoded and saved to the "Encoded" file
            for (int i = 0; i < messageLength; i++)
            {
                string temp = "";
                short[] t = Encode(messageText[i]);

                //save also binary representation
                short letter = ConvertTwoBytesToASCII(t);
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

            Console.WriteLine("If you wish, simulate errors in the encoded file");
            Console.ReadKey();
            Console.Clear();

            #region decode
            string decodedWord = "";
            string[] codeWords = File.ReadAllLines(encodedFilePath);
            short[] intCodeWords = new short[n];

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

            Console.WriteLine("Full decoded word: " + decodedWord);
            #endregion

            Console.ReadKey();
        }

        //Multiply the Hamming matrix by a code vector
        public static short[] MultiplyByHamming(short[] vector)
        {
            int[] errorTable = new int[m];
            short[] result = new short[m];
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

        //Encode a character to its code representation
        public static short[] Encode(char character)
        {
            short[] table = new short[n];
            short[] errorTable = new short[m];
            int ascii = Convert.ToInt32(character);
            table = ASCIItoBinary(ascii);
            errorTable = MultiplyByHamming(table);
            for (int i = 8, j = 0; i < n; i++, j++)
            {
                table[i] = errorTable[j];
            }
            return table;
        }

        //Decodes the binary code to ASCII
        public static char Decode(short[] codedTable)
        {
            short[] errorTable = MultiplyByHamming(codedTable);
            if (CheckError(errorTable))
            {
                int x = OneError(errorTable);
                if (x != -1)
                {
                    Console.Write("Bit no." + x + " was scrambled: ");
                    codedTable[x] += 1;
                    codedTable[x] %= 2;
                }
                else
                {
                    if (TwoErrors(errorTable)[0] != -1)
                    {
                        var errorIndexes = TwoErrors(errorTable);
                        Console.Write("Bits no." + errorIndexes[0] + " and " + errorIndexes[1] + " were scrambled: ");
                        codedTable[errorIndexes[0]] += 1;
                        codedTable[errorIndexes[0]] %= 2;
                        codedTable[errorIndexes[1]] += 1;
                        codedTable[errorIndexes[1]] %= 2;
                    }
                    else
                    {
                        Console.Write("Too many bits were scrambled ");
                    }
                }
            }
            return Convert.ToChar(ConvertToASCII(codedTable));
        }

        //Check if any column corresponds to the error vector and returns the column's index
        //If impossible, return -1
        public static int OneError(short[] errorTable)
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

        //Analogous to the previous method, only checks for the boolean sum of two columns
        public static int[] TwoErrors(short[] errorTable)
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

        //Convert one byte of to ascii
        public static int ConvertToASCII(short[] table)
        {
            double code = 0.0;
            for (int i = 0, j = 7; i < m; i++, j--)
            {
                code += (table[i] * Math.Pow(2.0, j));
            }
            return (int)code;
        }

        public static short ConvertTwoBytesToASCII(short[] table)
        {
            double code = 0.0;
            for (int i = 0, j = 15; i < n; i++, j--)
            {
                code += (table[i] * Math.Pow(2.0, j));
            }
            return (short)code;
        }

        //Convert an ASCII character to its binary representation
        public static short[] ASCIItoBinary(int character)
        {
            int temp;
            short[] retTable = new short[n];
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

        //Check if error table is correct
        public static bool CheckError(short[] errorTable)
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