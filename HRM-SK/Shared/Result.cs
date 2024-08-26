﻿namespace HRM_SK.Shared
{
    public class Result
    {
        protected internal Result(bool isSuccess, Error error)
        {
            if (isSuccess && error != Error.None)
            {
                throw new InvalidOperationException();
            }
            if ((error?.Message is not null) && error?.Message is FluentValidation.Results.ValidationResult)
            {
                isValidationError = true;
            }

            if (!isSuccess && error == Error.None)
            {
                throw new InvalidOperationException();
            }

            IsSuccess = isSuccess;
            Error = error;
        }

        public bool IsSuccess { get; }

        public bool isValidationError { get; private set; } = false;
        public bool IsFailure => !IsSuccess;

        public Error Error { get; }

        public static Result Success() => new(true, Error.None);

        public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

        public static Result Failure(Error error) => new(false, error);

        public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);

        public static Result<TValue> InvalidRequest<TValue>() => Failure<TValue>(Error.InvalidRequest);

        public static Result InvalidRequest() => Failure(Error.InvalidRequest);

        public static Result Create(bool condition) => condition ? Success() : Failure(Error.ConditionNotMet);

        public static Result<TValue> Create<TValue>(TValue? value) => value is not null ? Success(value) : Failure<TValue>(Error.NullValue);


    }
}
