﻿using System;

namespace AggregateSource.Testing
{
    /// <summary>
    /// The result of an exception centric constructor command test specification.
    /// </summary>
    public class ExceptionCentricAggregateConstructorTestResult
    {
        readonly ExceptionCentricAggregateConstructorTestSpecification _specification;
        readonly TestResultState _state;
        readonly Optional<Exception> _actualException;
        readonly Optional<object[]> _actualEvents;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionCentricTestResult"/> class.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <param name="state">The state.</param>
        /// <param name="actualException">The actual exception.</param>
        /// <param name="actualEvents">The actual events.</param>
        internal ExceptionCentricAggregateConstructorTestResult(ExceptionCentricAggregateConstructorTestSpecification specification, TestResultState state,
                                                                Exception actualException = null,
                                                                object[] actualEvents = null)
        {
            _specification = specification;
            _state = state;
            _actualException = actualException == null
                                   ? Optional<Exception>.Empty
                                   : new Optional<Exception>(actualException);
            _actualEvents = actualEvents == null
                                ? Optional<object[]>.Empty
                                : new Optional<object[]>(actualEvents);
        }

        /// <summary>
        /// Gets the test specification associated with this result.
        /// </summary>
        /// <value>
        /// The test specification.
        /// </value>
        public ExceptionCentricAggregateConstructorTestSpecification Specification
        {
            get { return _specification; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ExceptionCentricAggregateConstructorTestResult"/> has passed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if passed; otherwise, <c>false</c>.
        /// </value>
        public bool Passed
        {
            get { return _state == TestResultState.Passed; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ExceptionCentricAggregateConstructorTestResult"/> has failed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if failed; otherwise, <c>false</c>.
        /// </value>
        public bool Failed
        {
            get { return _state == TestResultState.Failed; }
        }

        /// <summary>
        /// Gets the exception that happened instead of the expected one, or empty if passed.
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        public Optional<Exception> But
        {
            get { return _actualException; }
        }

        /// <summary>
        /// Gets the events that happened instead of the expected exception, or empty if passed.
        /// </summary>
        /// <value>
        /// The events.
        /// </value>
        public Optional<object[]> Buts
        {
            get { return _actualEvents; }
        }
    }
}