using System;
using System.Diagnostics.Contracts;
using System.Collections.Generic;
using LiquidState.Common;
using LiquidState.Core;

namespace LiquidState.Synchronous.Core
{
    internal static class ExecutionHelper
    {
        internal static void ThrowInTransition()
        {
            throw new InvalidOperationException("State cannot be changed while already in transition. Tip: Use an asynchronous state machine such as QueuedStateMachine that has these parallel semantics for these to work out of the box.");
        }

        internal static bool CheckFlag(TransitionFlag source, TransitionFlag flagToCheck)
        {
            return (source & flagToCheck) == flagToCheck;
        }

        internal static void ExecuteAction(Action action)
        {
            if (action != null) action.Invoke();
        }

        internal static StateRepresentation<TState, TTrigger> FindStateRepresentation<TState, TTrigger>(TState initialState, Dictionary<TState, StateRepresentation<TState, TTrigger>> representations)
        {
            StateRepresentation<TState, TTrigger> rep;
            return representations.TryGetValue(initialState, out rep) ? rep : null;
        }

        internal static void MoveToStateCore<TState, TTrigger>(TState state, StateTransitionOption option, RawStateMachineBase<TState, TTrigger> machine)
        {

            Contract.Requires(machine != null);

            StateRepresentation<TState, TTrigger> targetRep;
            if (machine.Representations.TryGetValue(state, out targetRep))
            {

                var currentRep = machine.CurrentStateRepresentation;
                machine.RaiseTransitionStarted(targetRep.State);

                if ((option & StateTransitionOption.CurrentStateExitTransition) ==
                    StateTransitionOption.CurrentStateExitTransition)
                {
                    ExecuteAction(currentRep.OnExitAction);
                }
                if ((option & StateTransitionOption.NewStateEntryTransition) ==
                    StateTransitionOption.NewStateEntryTransition)
                {
                    ExecuteAction(targetRep.OnEntryAction);
                }

                var pastState = currentRep.State;
                machine.CurrentStateRepresentation = targetRep;
                machine.RaiseTransitionExecuted(pastState);
            }
            else
            {
               machine.RaiseInvalidState(state);
            }
        }

        internal static bool CanHandleTriggerCore<TState, TTrigger>(TTrigger trigger, RawStateMachineBase<TState, TTrigger> machine)
        {
            Contract.Requires(machine != null);

            var triggerRep = StateConfigurationHelper<TState, TTrigger>.FindTriggerRepresentation(trigger,
                machine.CurrentStateRepresentation);

            if (triggerRep == null)
                return false;

            var predicate = triggerRep.ConditionalTriggerPredicate;
            return predicate == null || predicate();
        }
        
        internal static TriggerRepresentation<TTrigger, TState> FindAndEvaluateTriggerRepresentation<TState, TTrigger>(TTrigger trigger, RawStateMachineBase<TState, TTrigger> machine)
        {
            Contract.Requires(machine != null);

            var triggerRep = StateConfigurationHelper<TState, TTrigger>.FindTriggerRepresentation(trigger,
                machine.CurrentStateRepresentation);

            if (triggerRep == null)
            {
                machine.RaiseInvalidTrigger(trigger);
                return null;
            }


            var predicate = triggerRep.ConditionalTriggerPredicate;
            if (predicate != null)
            {
                if (!predicate())
                {
                    machine.RaiseInvalidTrigger(trigger);
                    return null;
                }
            }

            // Handle ignored trigger

            if (triggerRep.NextStateRepresentationPredicate == null)
            {
                return null;
            }

            return triggerRep;
        }

        internal static void FireCore<TState, TTrigger>(TTrigger trigger, RawStateMachineBase<TState, TTrigger> machine)
        {
            Contract.Requires(machine != null);

            var currentStateRepresentation = machine.CurrentStateRepresentation;
            var triggerRep = FindAndEvaluateTriggerRepresentation(trigger, machine);
            if (triggerRep == null)
                return;

            // Catch invalid paramters before execution.

            Action triggerAction = null;
            try
            {
                triggerAction = (Action)triggerRep.OnTriggerAction;
            }
            catch (InvalidCastException)
            {
                machine.RaiseInvalidTrigger(trigger);
            }

            StateRepresentation<TState, TTrigger> nextStateRep = null;

            if (CheckFlag(triggerRep.TransitionFlags,
                TransitionFlag.DynamicState))
            {
                var state = ((Func<TState>) triggerRep.NextStateRepresentationPredicate)();
                nextStateRep = FindStateRepresentation(state, machine.Representations);
                if (nextStateRep == null)
                {
                    machine.RaiseInvalidState(state);
                    return;
                }
            }
            else
            {
                nextStateRep = (StateRepresentation<TState, TTrigger>) triggerRep.NextStateRepresentationPredicate;
            }

            machine.RaiseTransitionStarted(nextStateRep.State);

            // Current exit
            var currentExit = currentStateRepresentation.OnExitAction;
            ExecuteAction(currentExit);

            // Trigger entry
            ExecuteAction(triggerAction);

            // Next entry
            var nextEntry = nextStateRep.OnEntryAction;
            ExecuteAction(nextEntry);

            var pastState = machine.CurrentState;
            machine.CurrentStateRepresentation = nextStateRep;
            machine.RaiseTransitionExecuted(pastState);
        }

        internal static void FireCore<TState, TTrigger, TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger, TArgument argument, RawStateMachineBase<TState, TTrigger> machine)
        {
            Contract.Requires(machine != null);

            var currentStateRepresentation = machine.CurrentStateRepresentation;
            var trigger = parameterizedTrigger.Trigger;

            var triggerRep = FindAndEvaluateTriggerRepresentation(trigger, machine);
            if (triggerRep == null)
                return;

            // Catch invalid parameters before execution.

            Action<TArgument> triggerAction = null;
            try
            {
                triggerAction = (Action<TArgument>)triggerRep.OnTriggerAction;
            }
            catch (InvalidCastException)
            {
                machine.RaiseInvalidTrigger(trigger);
            }

            StateRepresentation<TState, TTrigger> nextStateRep = null;

            if (CheckFlag(triggerRep.TransitionFlags,
                TransitionFlag.DynamicState))
            {
                var state = ((Func<TState>)triggerRep.NextStateRepresentationPredicate)();
                nextStateRep = FindStateRepresentation(state, machine.Representations);
                if (nextStateRep == null)
                {
                    machine.RaiseInvalidState(state);
                    return;
                }
            }
            else
            {
                nextStateRep = (StateRepresentation<TState, TTrigger>)triggerRep.NextStateRepresentationPredicate;
            } 
            
            machine.RaiseTransitionStarted(nextStateRep.State);

            // Current exit
            var currentExit = currentStateRepresentation.OnExitAction;
            ExecuteAction(currentExit);

            // Trigger entry
            if (triggerAction != null) triggerAction.Invoke(argument);


            // Next entry
            var nextEntry = nextStateRep.OnEntryAction;
            ExecuteAction(nextEntry);

            var pastState = machine.CurrentState;
            machine.CurrentStateRepresentation = nextStateRep;
            machine.RaiseTransitionExecuted(pastState);
        }
    }
}
