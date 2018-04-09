using System;
using System.IO;

using Microsoft.Extensions.Configuration;

using TradeBot.Utils.ExtensionMethods;

namespace TradeBot
{
	class Program
	{
		public static IConfiguration Configuration { get; set; }


		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");

			//Can_Get_Values_From_AppSettings_Dot_Json();

			Can_Determine_The_Correct_Number_Of_Contracts();

			Console.ReadKey();

		}

		private static void Can_Get_Values_From_AppSettings_Dot_Json()
		{
			/**
			 * Testing to try to get values from appsettings.json (ran into issue when unable to get value of object in array other than by index)
			 **/

			var builder = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("appsettings.json");

			Configuration = builder.Build();

			//Console.WriteLine($"option1 = {Configuration["Option1"]}");
			//Console.WriteLine($"option2 = {Configuration["option2"]}");
			//Console.WriteLine(
			//	$"suboption1 = {Configuration["subsection:suboption1"]}");
			//Console.WriteLine();

			//Console.WriteLine("Wizards:");
			//Console.Write($"{Configuration["wizards:0:Name"]}, ");
			//Console.WriteLine($"age {Configuration["wizards:0:Age"]}");
			//Console.Write($"{Configuration["wizards:1:Name"]}, ");
			//Console.WriteLine($"age {Configuration["wizards:1:Age"]}");
			//Console.WriteLine();

			//Console.WriteLine("Press a key...");
			//Console.ReadKey();

			var accountValueQualifiers = Configuration["AccountValueQualifiers:0:Value"];
		}

		private static void Can_Determine_The_Correct_Number_Of_Contracts()
		{
			double optionBuyingPower = 500;
			double positionPrice = 1000;

			double maxContracts = optionBuyingPower / positionPrice;

			Console.WriteLine($"Value: {maxContracts.ToGetBase()}");
		}
	}

}
