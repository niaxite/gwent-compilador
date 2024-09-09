using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static void Main()
    {
        string carpetaRuta = @"C:\Users\Enrique Hidalgo\Documents\GitHub\gwent compilador\test text";
        
        string[] archivos = Directory.GetFiles(carpetaRuta, "*.txt");

        if (archivos.Length == 0)
        {
            Console.WriteLine("No se encontraron archivos de prueba.");
            return;
        }

        foreach (var archivo in archivos)
        {
            Console.WriteLine($"Procesando archivo: {Path.GetFileName(archivo)}");

            string texto;
            try
            {
                texto = File.ReadAllText(archivo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al leer el archivo {archivo}: {ex.Message}");
                continue;
            }

            List<Ttokens> ttokens;
            try
            {
                ttokens = Lexerr.Tokengen(texto);
                TtokenPrinter(ttokens);
            }
            catch (Lexernonvalid ex)
            {
                Console.WriteLine($"Error durante la tokenización: {ex.Message}");
                continue;
            }

            var pparser = new Pparser(ttokens);
            List<ASTNode> ast;
            try
            {
                ast = pparser.Parse().Where(node => node != null).ToList();
            }
            catch (Pparsernonvalid ex)
            {
                Console.WriteLine($"Error al parsear: {ex.Message}");
                continue;
            }

           
            Console.WriteLine("\nAST:");
            var printer = new ASTPrinter();
            foreach (var node in ast)
            {
                string result = node.Accept(printer);
                Console.WriteLine(result);
                Console.WriteLine(); 
            }


            Console.WriteLine("Iniciando análisis semántico.");
            var semanticAnalyzer = new SemanticAnalyzer();
            try
            {
                foreach (var node in ast)
                {
                    node.Accept(semanticAnalyzer);
                }
                Console.WriteLine("Análisis semántico completado exitosamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error durante el análisis semántico: {ex.Message}");
                continue;
            }

            Console.WriteLine($"Archivo {Path.GetFileName(archivo)} procesado exitosamente.\n");
        }

        Console.WriteLine("Todos los archivos han sido procesados.");
    }

    static void TtokenPrinter(List<Ttokens> tokens)
    {
        Console.WriteLine("Tokens:");
        foreach (var token in tokens)
        {
            Console.WriteLine(token);
        }
    }
}