using SeleniumShield.Driver.Logging;

namespace SeleniumShield.Driver
{
    public class SeleniumShieldDriverOptions
    {
        /// <summary>
        /// Gets or sets the default timeout (in seconds) used for the driver actions.
        /// The driver will retry an action until the timeout expires.
        /// </summary>
        public double DefaultTimeoutInSeconds { get; set; }

        /// <summary>
        /// Gets or sets the default timeout (in seconds) used for the driver actions.
        /// The driver will retry an action until the timeout expires.
        /// </summary>
        public double DefaultRetryDelayInSeconds { get; set; }

        /// <summary>
        /// Gets or sets the default maximal number of retry attempts allowed for a
        /// retry block with a limit on the number of attempts, rathern than a timeout
        /// </summary>
        public int DefaultMaxAllowedRetryAttempts { get; set; }

        /// <summary>
        /// Gets or sets the time that the driver will pause before any action is taken.
        /// Leave as zero for full speed.
        /// This can be useful when debugging a failing test to slow down the action sequence.
        /// </summary>
        public int SleepTimeBetweenActionsInMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the base url of the site under test, for example http://localhost/mysite
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the path to the directory where screenshots, logs etc. are stored by the driver
        /// </summary>
        public string OutputDirectoryPath { get; set; }

        /// <summary>
        /// Gets or sets the logger to which errors and debug information is written
        /// </summary>
        public ISeleniumShieldDriverLogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the number of retries that are allowed for each checkpoint set by the test author.
        /// For example, if MaxRetryCountPerCheckpoint is 3, the driver will restore the state to the
        /// checkpoint state by using the restore action, and then replay the actions taken so far. This will
        /// be done at most three times. If the test still fails after that, then the exception is rethrown and
        /// the driver fails.
        /// </summary>
        public int MaxRetryCountPerCheckpoint { get; set; }
    }
}