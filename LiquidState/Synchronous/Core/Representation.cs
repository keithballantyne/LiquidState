﻿// Author: Prasanna V. Loganathar
// Created: 2:12 AM 27-11-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LiquidState.Synchronous.Core
{
    internal class StateRepresentation<TState, TTrigger>
    {
        public readonly TState State;
        public readonly List<TriggerRepresentation<TTrigger, TState>> Triggers;
        public Action OnEntryAction;
        public Action OnExitAction;

        internal StateRepresentation(TState state)
        {
            Contract.Requires(state != null);
            Contract.Ensures(State != null);

            State = state;
            // Allocate with capacity as 1 to avoid wastage of memory.
            Triggers = new List<TriggerRepresentation<TTrigger, TState>>(1);
        }
    }

    internal class TriggerRepresentation<TTrigger, TState>
    {
        public readonly TTrigger Trigger;
        public Func<bool> ConditionalTriggerPredicate;
        public object NextStateRepresentationPredicate;
        public object OnTriggerAction;
        public TransitionFlag TransitionFlags;

        internal TriggerRepresentation(TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Ensures(Trigger != null);

            Trigger = trigger;
        }
    }

    [Flags]
    internal enum TransitionFlag
    {
        None = 0,
        DynamicState = 1,
    }
}
