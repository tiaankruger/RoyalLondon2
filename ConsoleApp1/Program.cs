using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;
using System.Xml;

namespace ConsoleApp1
{
    class Program
    {
        //fees and dates stored in static variables so that it is one place, but cant be changed in code. In bigger program it would probably be in a seperate constants file/class
        static readonly int FeeA = 3;
        static readonly int FeeB = 5;
        static readonly int FeeC = 7;
        static readonly DateTime CriteriaDateA = DateTime.Parse("1990-01-01");
        static readonly DateTime CriteriaDateC = DateTime.Parse("1990-01-01");

        static void Main(string[] args)
        {
            Console.Write("Please provide the folder path where the MaturityData.csv file is");
            string fileLocation = Console.ReadLine();
            string curFile = fileLocation.Replace("\\MaturityData.csv", "") + "\\MaturityData.csv";                   //caters for cases where user inputs the full path, and not just the folder

            if (File.Exists(curFile))
            {
                Console.WriteLine("File exists. Proceeding");
                List<MaturityData> policies = File.ReadAllLines(curFile)
                                                .Skip(1)
                                               .Select(v => MaturityData.FromCsv(v)).ToList();

                Console.WriteLine("File read in, processing");
                foreach (MaturityData policy in policies)
                {
                    int applicableFee = 0;
                    int isBonusApplicable = 0;
                    string type = policy.policy_number.Substring(0, 1);

                    applicableFee = GetApplicableFee(type);
                    isBonusApplicable = IsBonusApplicable(policy, isBonusApplicable, type);
                    policy.maturityValue = CalculateMaturityValue(policy, applicableFee, isBonusApplicable);
                }
                Console.WriteLine("Processing completed, outputing file");

                DataToXML(policies, fileLocation);
            }
            else
            { Console.WriteLine("File does not exist. Program will now close"); }
            Console.Read();
        }

        private static double CalculateMaturityValue(MaturityData value, int applicableFee, int isBonusApplicable)
        {
            if (applicableFee != 0)
                return ((value.premiums / 100 * (100 - applicableFee)) + (isBonusApplicable * value.discretionary_bonus)) * value.uplift_percentage; //normal calculation
            else
                return 0;           //if policy number is invalid(doesnt start with A,B,C it returns 0 so invalid policy numbers can be quickly checked
        }

        private static void DataToXML(List<MaturityData> values,string fileLocation)
        {
            using (XmlWriter writer = XmlWriter.Create(fileLocation+"\\MaturityValue.xml"))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("PolicyData");
                foreach (MaturityData value in values)
                {
                    writer.WriteStartElement("Policy");
                    writer.WriteElementString("policy_number", value.policy_number);
                    writer.WriteElementString("MaturityValue", value.maturityValue.ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        private static int IsBonusApplicable(MaturityData value, int isBonusApplicable, string type)
        {
            if (type == "A" && value.policy_start_date < CriteriaDateA)
                isBonusApplicable = 1;
            else if (type == "B" && value.membership == "Y")
                isBonusApplicable = 1;
            else if (type == "C" && value.policy_start_date >= CriteriaDateA && value.membership == "Y")
                isBonusApplicable = 1;
            else isBonusApplicable = 0;
            return isBonusApplicable;
        }

        private static int GetApplicableFee( string type)
        {
            int applicableFee = 0;
            if (type == "A")
                applicableFee = FeeA;
            else if (type == "B")
                applicableFee = FeeB;
            else if (type == "C")
                applicableFee = FeeC;
            else applicableFee = 0;
            return applicableFee;
        }

        



        class MaturityData
        {

            public string policy_number;
            public DateTime policy_start_date;
            public double premiums;
            public string membership;
            public double discretionary_bonus;
            public double uplift_percentage;
            public double maturityValue;

            public static MaturityData FromCsv(string csvLine)
            {
                string[] values = csvLine.Split(',');
                MaturityData maturityData = new MaturityData();
                maturityData.policy_number = Convert.ToString(values[0]);
                maturityData.policy_start_date = DateTime.ParseExact(values[1], "dd/MM/yyyy", CultureInfo.InvariantCulture); 
                maturityData.premiums = Convert.ToDouble(values[2]);
                maturityData.membership = Convert.ToString(values[3]);
                maturityData.discretionary_bonus = Convert.ToDouble(values[4]);
                maturityData.uplift_percentage = Convert.ToDouble(values[5], new CultureInfo("en-US"));
                maturityData.maturityValue = 0;
                return maturityData;
            }
            
        }
    }
}
