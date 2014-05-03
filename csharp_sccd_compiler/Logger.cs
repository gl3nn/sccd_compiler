using System;

namespace csharp_sccd_compiler
{
    public class Logger
    {
        /// <summary>
        /// Gets or sets the verbose of the logger.
        /// </summary>
        /// <value>The verbose. -1 = no ouput; 0 = only errors; 1 = only warnings and errors; 2 = all output.</value>
        static public int verbose = 2;

        private Logger()
        {
        }

        public static void displayError(string message)
        {
            if (Logger.verbose > -1)
                Console.WriteLine(string.Format("ERROR : {0}", message));
        }

        public static void displayWarning(string message)
        {
            if (Logger.verbose > 0)
                Console.WriteLine(string.Format("WARNING : {0}", message));
        }

        public static void displayInfo(string message)
        {
            if (Logger.verbose > 1)
                Console.WriteLine(string.Format("INFO : {0}", message));
        }
    }
}