using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Math;
using static Math.MathEngine;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("a = 100");
            Console.WriteLine("dec is a custom function that reduces number by 1");
            Console.WriteLine();
            Console.WriteLine("Try inputing (a + 1)");
            Console.WriteLine("Try inputing dec(a + 1)");
            Console.WriteLine("Try inputing (dec(a - 1))*2");
            Console.WriteLine();

            while (true)
            {
                Console.Write("Expression: ");
                string expression = Console.ReadLine();

                MathEngine mathEngine = new MathEngine();
                ReturnType returnType = ReturnType.Double;

                //Add Variable To MathEngine
                mathEngine.Variables.Add("a",100);

                mathEngine.Other += MathFuc;

                mathEngine.Express(ref expression, returnType);

                Console.WriteLine(expression);
                Console.WriteLine();
            }
            

        }
        public static dynamic MathFuc(dynamic inp){
            //decrease by 1;
           double output = Convert.ToDouble(inp);
           output--;

           return output;
        }
    }
}
