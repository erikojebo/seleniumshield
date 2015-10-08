using System;

namespace SeleniumShield
{
    public class FailedAutomationStepExecution
    {
        public readonly DateTime FailureDateTime;
        public readonly Exception Exception;

        public FailedAutomationStepExecution(DateTime failureDateTime, Exception exception)
        {
            FailureDateTime = failureDateTime;
            Exception = exception;
        }
    }
}