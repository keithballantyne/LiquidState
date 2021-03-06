﻿// Author: Prasanna V. Loganathar
// Created: 2:12 AM 27-11-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System.Diagnostics.Contracts;
using LiquidState.Common;
using LiquidState.Core;
using LiquidState.Synchronous.Core;

namespace LiquidState.Synchronous
{
    public abstract class GuardedStateMachineBase<TState, TTrigger> : RawStateMachineBase<TState, TTrigger>
    {
        private InterlockedMonitor monitor = new InterlockedMonitor();

        protected GuardedStateMachineBase(TState initialState, Configuration<TState, TTrigger> configuration)
            : base(initialState, configuration)
        {
            Contract.Requires(configuration != null);
            Contract.Requires(initialState != null);
        }

        public override void MoveToState(TState state, StateTransitionOption option = StateTransitionOption.Default)
        {
            if (monitor.TryEnter())
            {
                try
                {
                    base.MoveToState(state, option);
                }
                finally
                {
                    monitor.Exit();
                }
            }
            else
            {
                if (IsEnabled)
                    ExecutionHelper.ThrowInTransition();
            }
        }

        public override void Fire<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger,
            TArgument argument)
        {
            if (monitor.TryEnter())
            {
                try
                {
                    base.Fire(parameterizedTrigger, argument);
                }
                finally
                {
                    monitor.Exit();
                }
            }
            else
            {
                if (IsEnabled)
                    ExecutionHelper.ThrowInTransition();
            }
        }

        public override void Fire(TTrigger trigger)
        {
            if (monitor.TryEnter())
            {
                try
                {
                    base.Fire(trigger);
                }
                finally
                {
                    monitor.Exit();
                }
            }
            else
            {
                if (IsEnabled)
                    ExecutionHelper.ThrowInTransition();
            }
        }
    }

    public sealed class GuardedStateMachine<TState, TTrigger> : GuardedStateMachineBase<TState, TTrigger>
    {
        public GuardedStateMachine(TState initialState, Configuration<TState, TTrigger> configuration)
            : base(initialState, configuration)
        {
            Contract.Requires(configuration != null);
            Contract.Requires(initialState != null);
        }
    }
}
