using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Connecterra
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = string.Empty;
            IEnumerable<Input> input = null;
            while (string.IsNullOrEmpty(filePath) || input == null || !input.Any())
            {
                Console.WriteLine("Please enter the file path with input");
                filePath = Console.ReadLine();
                input = GetInputFromFile(filePath);
                if (input == null || !input.Any())
                    Console.WriteLine("No input in filepath");
            }

            string merge = string.Empty;

            while (string.IsNullOrWhiteSpace(merge))
            {
                Console.WriteLine("Please enter the merge distance in integer");
                merge = Console.ReadLine();
                if (!merge.All(a => char.IsDigit(a))) merge = string.Empty;
            }
            int mergeDistance = Convert.ToInt32(merge);
            MergeInterval(input, mergeDistance);
        }

        private static IEnumerable<Input> GetInputFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                return File.ReadAllLines(filePath).Select(line => line.Split(','))
                    .Select(x => new Input()
                    {
                        Sequence = Convert.ToInt32(x[0]),
                        Start = Convert.ToInt32(x[1]),
                        End = Convert.ToInt32(x[2]),
                        Action = x[3]
                    });
            }
            return null;
        }

        private static void MergeInterval(IEnumerable<Input> input, int mergeDistance)
        {
            var outcome = new List<int[]>();

            foreach (var item in input)
            {
                if (item.Action.Equals("ADDED", StringComparison.InvariantCultureIgnoreCase))
                {
                    outcome = IsAdded(item, mergeDistance, outcome);
                    PrintOutcome(outcome);
                }
                else if (item.Action.Equals("DELETED", StringComparison.InvariantCultureIgnoreCase))
                {
                    outcome = IsDeleted(item, outcome);
                    PrintOutcome(outcome);
                }
                else
                {
                    outcome = IsRemoved(input, mergeDistance);
                    PrintOutcome(outcome);
                }
            }
        }

        private static List<int[]> IsAdded(Input input, int mergeDistance, List<int[]> outcome)
        {

            if (outcome.Any())
            {
                bool startIncluded = outcome.Any(a => IsWithin(input.Start, a[0], a[1]));
                bool endIncluded = outcome.Any(a => IsWithin(input.End, a[0], a[1]));
                if (startIncluded)
                {
                    if (!endIncluded)
                    {
                        int index = outcome.FindIndex(a => IsWithin(input.Start, a[0], a[1]));
                        if (index != -1)
                        {
                            outcome[index][1] = input.End;
                        }
                    }
                }
                else
                {
                    // check merge distance
                    // if within extend input.Start
                    // else add to list
                    bool isStartWithinMergeRange = outcome.Any(a => IsWithin(input.Start, a[0] - mergeDistance, a[0] + mergeDistance) || IsWithin(input.Start, a[1] - mergeDistance, a[1] + mergeDistance));
                    bool isEndWithinMergeRange = outcome.Any(a => IsWithin(input.End, a[0] - mergeDistance, a[0] + mergeDistance) || IsWithin(input.End, a[1] - mergeDistance, a[1] + mergeDistance));

                    if (isStartWithinMergeRange)
                    {
                        // pass true for start
                        UpdateOutcomeList(input, true, mergeDistance, outcome);
                    }
                    else if (isEndWithinMergeRange)
                    {
                        // pass false for end
                        UpdateOutcomeList(input, false, mergeDistance, outcome);
                    }
                    else
                    {
                        outcome.Add(new int[] { input.Start, input.End });
                    }
                }
            }
            else
                outcome.Add(new int[] { input.Start, input.End });


            return outcome;
        }

        private static List<int[]> IsRemoved(IEnumerable<Input> input, int mergeDistance)
        {
            var outcome = new List<int[]>();
            var removedItem = input.Where(a => a.Action.Equals("REMOVED", StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (removedItem != null && removedItem.Any())
            {
                input = input.Where(a => !removedItem.Any(b => b.Start == a.Start && b.End == a.End)).ToList();
            }
            foreach (var item in input)
            {
                outcome = IsAdded(item, mergeDistance, outcome);
            }

            return outcome;
        }

        private static List<int[]> IsDeleted(Input input, List<int[]> outcome)
        {
            if (outcome != null && outcome.Any())
            {
                bool startIncluded = outcome.Any(a => IsWithin(input.Start, a[0], a[1]));
                int temp;
                if (startIncluded)
                {
                    int index = outcome.FindIndex(a => IsWithin(input.Start, a[0], a[1]));
                    if (index != -1)
                    {
                        temp = outcome[index][1];
                        outcome[index][1] = input.Start;
                        outcome.Add(new int[] { input.End, temp });
                    }
                }
            }

            return outcome;
        }

        private static void UpdateOutcomeList(Input input, bool isStart, int mergeDistance, List<int[]> outcome)
        {
            int index = outcome.FindIndex(a => IsWithin(isStart ? input.Start : input.End, a[0] - mergeDistance, a[0] + mergeDistance) || IsWithin(isStart ? input.Start : input.End, a[1] - mergeDistance, a[1] + mergeDistance));

            if (index != -1)
            {
                // take the least
                outcome[index][0] = Math.Min(outcome[index][0], input.Start);
                outcome[index][1] = Math.Max(outcome[index][1], input.End);
            }
        }

        private static bool IsWithin(int number, int minimum, int maximum)
        {
            return number >= minimum && number <= maximum;
        }

        private static void PrintOutcome(List<int[]> outcome)
        {
            var outputstring = string.Empty;
            outcome = outcome.OrderBy(a => a[0]).ToList();
            foreach (var output in outcome)
            {
                outputstring += string.Format("[{0},{1}]", output[0], output[1]);
            }
            Console.WriteLine(outputstring);
        }
    }

    class Input
    {
        public int Sequence { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public string Action { get; set; }
    }
}
