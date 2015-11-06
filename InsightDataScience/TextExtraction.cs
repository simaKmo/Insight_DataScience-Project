using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace InsightDataScience
{
    class TextExtraction
    {
        /// <summary>
        /// I have considered three pathes for input and output files as constant
        /// </summary>
        private SlidingWindow _slidingWindows;
        private const string _filePath = @"..\\..\\coding-challenge-master\tweet_input\tweets.txt";
        private const string _outPutOnePath = @"..\\..\\coding-challenge-master\tweet_output\ft1.txt";
        private const string _outPutTwoPath = @"..\\..\\coding-challenge-master\tweet_output\ft2.txt";

        /// <summary>
        /// I am using the Newtonsoft.Json library to have easy access to json file attribures.
        /// for having Newtonsoft libarary I have used manage NuGet option of Refrence menue.
        /// </summary>
        public TextExtraction()
        {
            _slidingWindows = new SlidingWindow();

        }
        /// <summary>
        /// in this method I am trying to extarct two main attributes of each json row: text and create_at
        /// then it calculates the degree by the help of SlidingWindow class.
        /// 
        /// the another important option about writing and reading from the files.
        /// actually reagdring the clean and friendly coding rules it would be better to do these two tasks seperately
        /// but I prefere to do both in one function becuase I do not like to use extra memory to save the results. 
        /// on the other hands using events and delegates cuase many intra-communications between classes and functions which 
        /// cause more time and rsources consuming. 
        /// Finally:
        /// I am extracting the text and date 
        /// then I am writing them immidiately on output to have better memory management
        /// </summary>
        public void FindCleanTextAndDegree()
        {
            using (FileStream fs = File.Open(_filePath, FileMode.Open))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            using (StreamWriter sw1 = File.AppendText(_outPutOnePath))
            using (StreamWriter sw2 = File.AppendText(_outPutTwoPath))
            {
                string s;



                while ((s = sr.ReadLine()) != null)
                {
                    JObject json = JObject.Parse(s);
                    string text = string.Empty;
                    string date = string.Empty;
                    foreach (KeyValuePair<String, JToken> app in json)
                    {

                        string appName = app.Key;
                        if (appName == "created_at")
                            date = (String)app.Value;

                        else if (appName == "text")
                        {

                            text = (String)app.Value;
                            text = Regex.Replace(text, @"[^\u0000-\u007F]", string.Empty);
                            text = Regex.Replace(text, @"\n", " ");
                            text = Regex.Replace(text, @"\t", " ");

                            //replace sequence spaces with one space
                            RegexOptions options = RegexOptions.None;
                            Regex regex = new Regex(@"[ ]{2,}", options);
                            text = regex.Replace(text, @" ");


                            ///write on first output!
                            sw1.WriteLine(text + " (timestamp:" + date + ")");

                            List<string> tags = GetHashTaggedWords(text);


                            _slidingWindows.CheckSlidingWindowUpdate(tags, date);
                            double degree = _slidingWindows.GetDegree();
                            //write on second output
                            sw2.WriteLine(degree.ToString());

                        }


                    }

                }
            }

        }

        /// <summary>
        /// Get Hash Tags words from text tags by the help of Regular Expression
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private List<string> GetHashTaggedWords(string text)
        {

            if (text != "")
            {
                var regex = new Regex(@"(?<=#)\w+");
                var matches = regex.Matches(text);
                if (matches != null && matches.Count > 0)
                {
                    List<string> tags = new List<string>();
                    foreach (Match m in matches)
                        if (!tags.Contains(m.Value))
                            tags.Add(m.Value);

                    return tags;
                }
            }
            return null;
        }



    }
}
