using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace AsyncApp
{
    class Program
    {
        async static Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Асинхронная реализация:");
                await UsingAsync();
                
                for (int i = 0; i < 70; i++)
                    Console.Write("-");

                Console.WriteLine("\nИспользование потока данных:");
                UsingDataFlow(5);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void UsingDataFlow()
        {
            var generationNumber = new TransformBlock<bool, int>(async flag =>
            {
                var result = await AsyncMethod.generateNumberAsync();
                Console.WriteLine($"\nГенерация числа... {result}");
                return result;
            });

            var generationNumbers = new TransformBlock<int, List<double>>(async count =>
            {
                var result = await AsyncMethod.generateNumbersAsync(count);
                Console.WriteLine($"\nГенерация списка...");
                result.ForEach((item) => Console.Write(item.ToString("f2") + "; "));
                Console.WriteLine();
                return result;
            });

            var calculateSum = new TransformBlock<List<double>, double>(async numerics =>
            {
                var result = await AsyncMethod.calcSumAsync(numerics);
                Console.WriteLine($"\nСумма элементов списка... {result.ToString("f2")}");
                return result;
            });

            var transformToString = new TransformBlock<double, string>(async item =>
            {
                Console.WriteLine("\nПриведение к типу string...");
                return await AsyncMethod.transformToStringAsync(item);
            });
           
            var writeResult = new ActionBlock<string>(item =>
            {
                Console.WriteLine("\nResult: " + item);
            });

            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            generationNumber.LinkTo(generationNumbers, linkOptions);
            generationNumbers.LinkTo(calculateSum, linkOptions);
            calculateSum.LinkTo(transformToString, linkOptions);
            transformToString.LinkTo(writeResult);

            generationNumber.Post(true);

            generationNumber.Complete();

            writeResult.Completion.Wait();
        }
        async static Task UsingAsync()
        {
            // определяем и запускаем задачи
            var result1 = await AsyncMethod.generateNumberAsync();
            Console.WriteLine("\nResult 1: " + result1);
            var result2 = await AsyncMethod.generateNumbersAsync(result1);
            Console.WriteLine("\nResult 2: ");
            result2.ForEach((item) => Console.Write(item.ToString("f2") + "; "));
            var result3 = await AsyncMethod.calcSumAsync(result2);
            Console.WriteLine("\n\nResult 3: " + result3.ToString("f2"));
            var result4 = await AsyncMethod.transformToStringAsync(result3);
            Console.WriteLine("\nResult 4: " + result4);
        }
    }

    class AsyncMethod
    {
        static public async Task<int> generateNumberAsync()
        {
            return await Task.Run(() => { return MyMethod.generateNumber(); });
        }
        static public async Task<List<double>> generateNumbersAsync(int count)
        {
            return await Task.Run(() => { return MyMethod.generateNumbers(count); });
        }

        static public async Task<double> calcSumAsync(List<double> numerics)
        {
            return await Task.Run(() => { return MyMethod.calcSum(numerics); });
        }

        static public async Task<string> transformToStringAsync(double number)
        {
            return await Task.Run(() => { return MyMethod.transformToString(number); });
        }
    }
    
    class MyMethod
    {
        static public int generateNumber()
        {
            return new Random().Next(10, 25);
        }
        static public List<double> generateNumbers(int count)
        {
                return Enumerable.Range(0, count).Select(i => new Random().Next(10, 25) + new Random().NextDouble()).ToList();
        }
        
        static public double calcSum(List<double> numerics)
        {
                return numerics.Sum();
        }

        static public string transformToString(double count)
        {
                 return count.ToString("f2");
        }
    }
}
