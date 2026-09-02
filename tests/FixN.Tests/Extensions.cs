using FluentAssertions.Execution;
using FluentAssertions.Numeric;

namespace FixN.Core.Tests;

public static class Extensions
{
    public static NumericAssertions<Fix> Should(this Fix actualValue) => new(actualValue);

    extension(NullableNumericAssertions<Fix> parent)
    {
        public AndConstraint<NullableNumericAssertions<Fix>> BeApproximately(
            Fix expectedValue, string because = "", params object[] becauseArgs) =>
            parent.BeApproximately(expectedValue, Fix.Epsilon, because, becauseArgs);

        public AndConstraint<NullableNumericAssertions<Fix>> BeApproximately(Fix expectedValue, Fix precision,
            string because = "",
            params object[] becauseArgs)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(precision);

            var success = Execute.Assertion
                .ForCondition(parent.Subject is not null)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:value} to approximate {0} +/- {1}{reason}, but it was <null>.",
                    expectedValue, precision);

            if (success)
            {
                NumericAssertions<Fix> nonNullableAssertions = new(parent.Subject!.Value);
                nonNullableAssertions.BeApproximately(expectedValue, precision, because, becauseArgs);
            }

            return new(parent);
        }
    }

    extension(NumericAssertions<Fix> parent)
    {
        public AndConstraint<NumericAssertions<Fix>> BeApproximately(
            Fix expectedValue, Fix precision,
            string because = "",
            params object[] becauseArgs
        )
        {
            ArgumentOutOfRangeException.ThrowIfNegative(precision);

            Fix actualDifference = Fix.Diff(expectedValue, parent.Subject!.Value);

            Execute.Assertion
                .ForCondition(actualDifference <= precision)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:value} to approximate {1} +/- {2}{reason}, but {0} differed by {3}.",
                    parent.Subject, expectedValue, precision, actualDifference);

            return new(parent);
        }

        public AndConstraint<NumericAssertions<Fix>> BeApproximately(Fix expectedValue, string because = "",
            params object[] becauseArgs) =>
            parent.BeApproximately(expectedValue, Fix.Epsilon, because, becauseArgs);
    }
}
