namespace DiscordBotProject;

public static class ConsoleWriter
{
    public static readonly object MessageLock = new();
    private static bool _isProgress = false;
    private static DateTime? _lastProgressStamp;
    private static double? _lastProgressFraction;

    private static double _averageFractionProgressPerSecond = 0;

    private static void MyWrite(
        string prefix,
        ConsoleColor prefixColor,
        string message,
        ConsoleColor messageColor,
        ConsoleColor? prefixBackgroundColor = null,
        ConsoleColor? messageBackgroundColor = null)
    {
        lock (MessageLock)
        {
            _isProgress = false;
            var previousBackground = Console.BackgroundColor;
            var previousForeground = Console.ForegroundColor;
            
            prefixBackgroundColor ??= previousBackground;
            messageBackgroundColor ??= previousForeground;
            
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.Write($"[{DateTime.Now}] ");

            Console.BackgroundColor = (ConsoleColor)prefixBackgroundColor;
            Console.ForegroundColor = prefixColor;
            Console.Write(prefix);
            Console.BackgroundColor = (ConsoleColor)messageBackgroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(":");
            // Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" ");

            Console.BackgroundColor = (ConsoleColor)messageBackgroundColor;
            Console.ForegroundColor = messageColor;
            Console.Write(message);

            Console.ForegroundColor = previousBackground;
            Console.BackgroundColor = previousBackground;
        }
    }

    /*private static void MyWriteLn(string prefix, ConsoleColor consoleColor, string message, ConsoleColor messageColor)
        => MyWrite(prefix, consoleColor, message + "\n", messageColor);*/

    public static void WriteInfo(string message)
        => MyWrite("info", ConsoleColor.Green, message, ConsoleColor.White);
    public static void WriteInfoLn(string message)
        => WriteInfo(message + "\n");

    public static void WriteDanger(string message)
        => MyWrite("!!!!", ConsoleColor.White, message, ConsoleColor.Red, prefixBackgroundColor: ConsoleColor.Red);
    public static void WriteDangerLn(string message)
        => WriteDanger(message + "\n");

    public static void WriteWarning(string message)
        => MyWrite("warn", ConsoleColor.Black, message, ConsoleColor.White, prefixBackgroundColor: ConsoleColor.DarkYellow);
    public static void WriteWarningLn(string message)
        => WriteWarning(message + "\n");

    public static void WriteSuccess(string message)
        => MyWrite("done", ConsoleColor.Green, message, ConsoleColor.Green);
    public static void WriteSuccessLn(string message)
        => WriteSuccess(message + "\n");

    public static string Prompt(string question)
    {
        MyWrite("prmt", ConsoleColor.White, question + " ", ConsoleColor.DarkBlue, ConsoleColor.DarkMagenta);
        Console.Out.Flush();
        return Console.ReadLine()!;
    }

    public static void WriteProgress(double current, double total, string additionalMessage = "", int width = 20)
    {
        lock (MessageLock)
        {
            if (!_isProgress)
            {
                _lastProgressStamp = DateTime.Now;
                _lastProgressFraction = 0;
                
                Console.WriteLine();
                _isProgress = true;
            }
            
            Console.Write("\r");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            
            Console.Write("prog: [");
            var fractionDone = current / total;
            var progressDoneWidth = (int)Math.Ceiling(width * fractionDone);
            Console.Write(new string('#', progressDoneWidth));
            Console.Write(new string('.', width - progressDoneWidth));
            
            Console.Write($"] {current}/{total} ({fractionDone:P})");

            if (additionalMessage != "")
            {
                Console.Write($"   {additionalMessage}");
            }

            // var timeDiff = DateTime.Now - _lastProgressStamp;
            // var fractionDiff = fractionDone - _lastProgressFraction;
            //
            // var newFractionProgressPerSecond = (1000000.0 / timeDiff!.Value.Microseconds) * fractionDiff;
            // _averageFractionProgressPerSecond = (double)((_averageFractionProgressPerSecond + newFractionProgressPerSecond) / 2.0)!;
            //
            // if (_averageFractionProgressPerSecond != 0)
            // {
            //     Console.Write($"   ~ {(1.0 - fractionDone) / _averageFractionProgressPerSecond} секунд            ");
            // }
            // else
            // {
            //     Console.Write($"                                     ");
            //
            // }
            //
            // _lastProgressFraction = fractionDone;
            // _lastProgressStamp = DateTime.Now;
        }
    }
}