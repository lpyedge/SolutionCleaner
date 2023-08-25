using System.Text.RegularExpressions;

namespace SolutionCleaner
{
    public class Program
    {
        public class Rule
        {
            public bool IsFile { get; set; } = false;

            /// <summary>
            /// regex
            /// </summary>
            public string Expression { get; set; } = "";

            /// <summary>
            /// 同级锚
            /// </summary>
            public Rule? Anchor { get; set; } = null;
        }

        public static List<Rule> Rules { get; set; } = new List<Rule>()
        {
            new Rule() { IsFile = false, Expression = @".*\\\.vs$"},
            new Rule() { IsFile = false, Expression = @".*\\bin$", Anchor= new Rule{IsFile=true,Expression = @".*\.(cs|vb|fs|vcx|py|js|xcode)proj$" }},
            new Rule() { IsFile = false, Expression = @".*\\obj$", Anchor= new Rule{IsFile=true,Expression = @".*\.(cs|vb|fs|vcx|py|js|xcode)proj$" }},

            new Rule() { IsFile = true, Expression = @".*\.rsuser$"},
            new Rule() { IsFile = true, Expression = @".*\.suo$"},
            new Rule() { IsFile = true, Expression = @".*\.userosscache$"},
            new Rule() { IsFile = true, Expression = @".*\.sln\.docstates$"},
            new Rule() { IsFile = true, Expression = @".*\.nupkg$"},
            new Rule() { IsFile = true, Expression = @".*\.snupkg$"},
        };

        public static string RegexExpressionFix(string expression)
        {
            return expression.Replace("*", ".*");
        }


        public static void Main(string[] args)
        {
            for (int i = 3; i > 0; i--)
            {
                Console.Clear();
                Console.WriteLine($"SolutionCleaner Running ... Waiting {i} s");
                Thread.Sleep(1000);
            }
            Console.Clear();
            Console.WriteLine($"SolutionCleaner Start:");

            var list = new List<string>();

            var baseDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            //baseDirectory = new DirectoryInfo("C:\\Source\\midorilife_New");

            foreach (var dirInfo in baseDirectory.GetDirectories("*", SearchOption.AllDirectories))
            {
                if (dirExec(dirInfo.FullName))
                {
                    continue;
                }
                if(Rules.Any(x => x.IsFile ==true)){
                    foreach (var filePath in Directory.GetFiles(dirInfo.FullName))
                    {
                        if (fileExec(filePath))
                        {
                            continue;
                        }
                    }
                }
            }

            Console.WriteLine("SolutionCleaner OK !!!");

            Console.ReadKey();
        }
        static bool fileExec(string filePath)
        {
            bool ismatch = false;
            foreach (var rule in Rules.Where(x => x.IsFile ==true))
            {
                ismatch = Regex.IsMatch(filePath, rule.Expression);
                if (ismatch && rule.Anchor != null)
                {
                    var path = filePath.Replace(rule.Expression.Replace(@"\\", @"\").Replace(".*", "").TrimEnd('$'), "");
                    if (rule.Anchor.IsFile)
                    {
                        ismatch = Directory.GetFiles(path).Any(x => Regex.IsMatch(x, rule.Anchor.Expression));
                    }
                    else
                    {
                        ismatch = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly).Any(x => Regex.IsMatch(x, rule.Anchor.Expression));
                    }
                }
                if (ismatch)
                {
#if !DEBUG
                    Directory.Delete(filePath);
#endif
                    Console.WriteLine(filePath);
                    break;
                }
            }
            return ismatch;
        }

        static bool dirExec(string dirPath)
        {
            bool ismatch = false;
            foreach (var rule in Rules.Where(x => x.IsFile ==false))
            {
                ismatch = Regex.IsMatch(dirPath, rule.Expression);
                if (ismatch && rule.Anchor != null)
                {
                    var path = dirPath.Replace(rule.Expression.Replace(@"\\", @"\").Replace(".*", "").TrimEnd('$'), "");
                    if (rule.Anchor.IsFile)
                    {
                        ismatch = Directory.GetFiles(path).Any(x => Regex.IsMatch(x, rule.Anchor.Expression));
                    }
                    else
                    {
                        ismatch = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly).Any(x => Regex.IsMatch(x, rule.Anchor.Expression));
                    }
                }
                if (ismatch)
                {
#if !DEBUG
                    Directory.Delete(dirPath);
#endif
                    Console.WriteLine(dirPath);
                    break;
                }
            }
            return ismatch;
        }
    }
}