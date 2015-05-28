// Author: Prasanna V. Loganathar
// Created: 04:16 11-05-2015
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using LiquidState.Core;

namespace LiquidState.Awaitable.Core
{
    public class StateConfigurationHelper<TState, TTrigger>
    {
        private readonly Dictionary<TState, StateRepresentation<TState, TTrigger>> config;
        private readonly StateRepresentation<TState, TTrigger> currentStateRepresentation;

        internal StateConfigurationHelper(
            Dictionary<TState, StateRepresentation<TState, TTrigger>> config,
            TState currentState)
        {
            Contract.Requires(config != null);
            Contract.Requires(currentState != null);

            Contract.Ensures(currentStateRepresentation != null);

            this.config = config;
            currentStateRepresentation = FindOrCreateStateRepresentation(currentState, config);
        }

        public StateConfigurationHelper<TState, TTrigger> OnEntry(Action action)
        {
            currentStateRepresentation.OnEntryAction = action;
            return this;
        }

        public StateConfigurationHelper<TState, TTrigger> OnEntry(Func<Task> asyncAction)
        {
            currentStateRepresentation.OnEntryAction = asyncAction;
            currentStateRepresentation.TransitionFlags |= TransitionFlag.EntryReturnsTask;

            return this;
        }

        public StateConfigurationHelper<TState, TTrigger> OnExit(Action action)
        {
            currentStateRepresentation.OnExitAction = action;
            return this;
        }

        public StateConfigurationHelper<TState, TTrigger> OnExit(Func<Task> asyncAction)
        {
            currentStateRepresentation.OnExitAction = asyncAction;
            currentStateRepresentation.TransitionFlags |= TransitionFlag.ExitReturnsTask;

            return this;
        }

        public StateConfigurationHelper<TState, TTrigger> PermitReentry(TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalSync(null, trigger, currentStateRepresentation.State, null);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitReentry(TTrigger trigger, Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalSync(null, trigger, currentStateRepresentation.State, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitReentry<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger, Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalSync(null, trigger, currentStateRepresentation.State, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitReentry(TTrigger trigger,
            Func<Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalTriggerAsync(null, trigger, currentStateRepresentation.State, onEntryAsyncAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitReentry<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalTriggerAsync(null, trigger, currentStateRepresentation.State, onEntryAsyncAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitReentryIf(Func<bool> predicate,
            TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalSync(predicate, trigger, currentStateRepresentation.State, null);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitReentryIf(Func<Task<bool>> predicate,
            TTrigger trigger)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalPredicateAsync(predicate, trigger, currentStateRepresentation.State, null);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitReentryIf(Func<bool> predicate,
            TTrigger trigger,
            Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalSync(predicate, trigger, currentStateRepresentation.State, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitReentryIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalSync(predicate, trigger, currentStateRepresentation.State, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitReentryIf(Func<Task<bool>> predicate,
            TTrigger trigger, Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalPredicateAsync(predicate, trigger, currentStateRepresentation.State, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitReentryIf<TArgument>(
            Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger, Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Assume(currentStateRepresentation.State != null);

            return PermitInternalPredicateAsync(predicate, trigger, currentStateRepresentation.State, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> Permit(TTrigger trigger, TState resultingState)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalSync(null, trigger, resultingState, null);
        }

        public StateConfigurationHelper<TState, TTrigger> Permit(TTrigger trigger, TState resultingState,
            Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalSync(null, trigger, resultingState, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> Permit<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger, TState resultingState,
            Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalSync(null, trigger, resultingState, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> Permit(TTrigger trigger, TState resultingState,
            Func<Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalTriggerAsync(null, trigger, resultingState, onEntryAsyncAction);
        }

        public StateConfigurationHelper<TState, TTrigger> Permit<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger, TState resultingState,
            Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalTriggerAsync(null, trigger, resultingState, onEntryAsyncAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitIf(Func<bool> predicate, TTrigger trigger,
            TState resultingState)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalSync(predicate, trigger, resultingState, null);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitIf(Func<Task<bool>> predicate, TTrigger trigger,
            TState resultingState)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalPredicateAsync(predicate, trigger, resultingState, null);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitIf(Func<bool> predicate, TTrigger trigger,
            TState resultingState, Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalSync(predicate, trigger, resultingState, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalSync(predicate, trigger, resultingState, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitIf(Func<Task<bool>> predicate, TTrigger trigger,
            TState resultingState, Action onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalPredicateAsync(predicate, trigger, resultingState, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitIf<TArgument>(Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<TArgument> onEntryAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalPredicateAsync(predicate, trigger, resultingState, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitIf(Func<bool> predicate, TTrigger trigger,
            TState resultingState, Func<Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalTriggerAsync(predicate, trigger, resultingState, onEntryAsyncAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalTriggerAsync(predicate, trigger, resultingState, onEntryAsyncAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitIf(Func<Task<bool>> predicate, TTrigger trigger,
            TState resultingState, Func<Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalAsync(predicate, trigger, resultingState, onEntryAsyncAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitIf<TArgument>(Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires(trigger != null);
            Contract.Requires(resultingState != null);

            return PermitInternalAsync(predicate, trigger, resultingState, onEntryAsyncAction);
        }

        public StateConfigurationHelper<TState, TTrigger> Ignore(TTrigger trigger)
        {
            Contract.Requires(trigger != null);

            return IgnoreInternal(null, trigger);
        }

        public StateConfigurationHelper<TState, TTrigger> IgnoreIf(Func<bool> predicate, TTrigger trigger)
        {
            Contract.Requires(trigger != null);

            return IgnoreInternal(predicate, trigger);
        }

        public StateConfigurationHelper<TState, TTrigger> IgnoreIf(Func<Task<bool>> predicate, TTrigger trigger)
        {
            Contract.Requires(trigger != null);

            return IgnoreInternalPredicateAsync(predicate, trigger);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitDynamic(TTrigger trigger,
            Func<TState> targetStatePredicate,
            Action onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStatePredicate != null);

            return PermitDynamicInternal(null, trigger, targetStatePredicate, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitDynamic<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<TState> targetStatePredicate,
            Action<TArgument> onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStatePredicate != null);

            return PermitDynamicInternal(null, trigger, targetStatePredicate, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitDynamic(TTrigger trigger,
            Func<TState> targetStatePredicate,
            Func<Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStatePredicate != null);

            return PermitDynamicInternalTriggerAsync(null, trigger, targetStatePredicate, onEntryAsyncAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitDynamic<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<TState> targetStatePredicate,
            Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStatePredicate != null);

            return PermitDynamicInternalTriggerAsync(null, trigger, targetStatePredicate, onEntryAsyncAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitDynamic(TTrigger trigger,
            Func<Task<TState>> targetStatePredicate,
            Action onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStatePredicate != null);

            return PermitDynamicAsyncInternal(null, trigger, targetStatePredicate, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitDynamic<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<Task<TState>> targetStatePredicate,
            Action<TArgument> onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStatePredicate != null);

            return PermitDynamicAsyncInternal(null, trigger, targetStatePredicate, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitDynamic(TTrigger trigger,
            Func<Task<TState>> targetStatePredicate,
            Func<Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStatePredicate != null);

            return PermitDynamicAsyncInternalTriggerAsync(null, trigger, targetStatePredicate, onEntryAsyncAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitDynamic<TArgument>(
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<Task<TState>> targetStatePredicate,
            Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStatePredicate != null);

            return PermitDynamicAsyncInternalTriggerAsync(null, trigger, targetStatePredicate, onEntryAsyncAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitDynamicIf(Func<bool> predicate,
            TTrigger trigger,
            Func<TState> targetStatePredicate,
            Action onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStatePredicate != null);

            return PermitDynamicInternal(predicate, trigger, targetStatePredicate, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitDynamicIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<TState> targetStatePredicate,
            Action<TArgument> onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStatePredicate != null);

            return PermitDynamicInternal(predicate, trigger, targetStatePredicate, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitDynamicIf(Func<Task<bool>> predicate,
            TTrigger trigger,
            Func<TState> targetStatePredicate,
            Action onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStatePredicate != null);

            return PermitDynamicInternalPredicateAsync(predicate, trigger, targetStatePredicate, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitDynamicIf<TArgument>(Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<TState> targetStatePredicate,
            Action<TArgument> onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStatePredicate != null);

            return PermitDynamicInternalPredicateAsync(predicate, trigger, targetStatePredicate, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitDynamicIf(Func<bool> predicate,
            TTrigger trigger,
            Func<TState> targetStatePredicate,
            Func<Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStatePredicate != null);

            return PermitDynamicInternalTriggerAsync(predicate, trigger, targetStatePredicate, onEntryAsyncAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitDynamicIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<TState> targetStatePredicate,
            Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStatePredicate != null);

            return PermitDynamicInternalTriggerAsync(predicate, trigger, targetStatePredicate, onEntryAsyncAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitDynamicIf(Func<bool> predicate,
            TTrigger trigger,
            Func<Task<TState>> targetStatePredicate,
            Action onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStatePredicate != null);

            return PermitDynamicAsyncInternal(predicate, trigger, targetStatePredicate, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitDynamicIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<Task<TState>> targetStatePredicate,
            Action<TArgument> onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStatePredicate != null);

            return PermitDynamicAsyncInternal(predicate, trigger, targetStatePredicate, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitDynamicIf(Func<Task<bool>> predicate,
            TTrigger trigger,
            Func<Task<TState>> targetStatePredicate,
            Action onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStatePredicate != null);

            return PermitDynamicAsyncInternalPredicateAsync(predicate, trigger, targetStatePredicate, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitDynamicIf<TArgument>(Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<Task<TState>> targetStatePredicate,
            Action<TArgument> onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStatePredicate != null);

            return PermitDynamicAsyncInternalPredicateAsync(predicate, trigger, targetStatePredicate, onEntryAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitDynamicIf(Func<bool> predicate,
            TTrigger trigger,
            Func<Task<TState>> targetStatePredicate,
            Func<Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStatePredicate != null);

            return PermitDynamicAsyncInternalTriggerAsync(predicate, trigger, targetStatePredicate, onEntryAsyncAction);
        }

        public StateConfigurationHelper<TState, TTrigger> PermitDynamicIf<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            Func<Task<TState>> targetStatePredicate,
            Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(targetStatePredicate != null);

            return PermitDynamicAsyncInternalTriggerAsync(predicate, trigger, targetStatePredicate, onEntryAsyncAction);
        }

        #region internal static routines
        internal static StateRepresentation<TState, TTrigger> FindOrCreateStateRepresentation(TState state,
                                                                Dictionary<TState, StateRepresentation<TState, TTrigger>> config)
        {
            Contract.Requires(state != null);
            Contract.Requires(config != null);

            Contract.Ensures(Contract.Result<StateRepresentation<TState, TTrigger>>() != null);

            StateRepresentation<TState, TTrigger> rep;
            if (config.TryGetValue(state, out rep))
            {
                if (rep != null) return rep;
            }
            rep = new StateRepresentation<TState, TTrigger>(state);

            config[state] = rep;

            return rep;
        }

        internal static StateRepresentation<TState, TTrigger> CreateStateRepresentation(TState state,
            Dictionary<TState, StateRepresentation<TState, TTrigger>> config)
        {
            Contract.Requires(state != null);
            var rep = new StateRepresentation<TState, TTrigger>(state);
            config[state] = rep;
            return rep;
        }

        internal static TriggerRepresentation<TTrigger, TState> FindOrCreateTriggerRepresentation(TTrigger trigger,
            StateRepresentation<TState, TTrigger> stateRepresentation)
        {
            Contract.Requires(stateRepresentation != null);
            Contract.Requires(trigger != null);

            Contract.Ensures(Contract.Result<TriggerRepresentation<TTrigger, TState>>() != null);

            var rep = FindTriggerRepresentation(trigger, stateRepresentation);
            return rep ?? CreateTriggerRepresentation(trigger, stateRepresentation);
        }

        internal static TriggerRepresentation<TTrigger, TState> CreateTriggerRepresentation(TTrigger trigger,
            StateRepresentation<TState, TTrigger> stateRepresentation)
        {
            Contract.Requires(stateRepresentation != null);
            var rep = new TriggerRepresentation<TTrigger, TState>(trigger);
            stateRepresentation.Triggers.Add(rep);
            return rep;
        }

        internal static TriggerRepresentation<TTrigger, TState> FindTriggerRepresentation(TTrigger trigger,
            StateRepresentation<TState, TTrigger> stateRepresentation)
        {
            return stateRepresentation.Triggers.Find(x => x.Trigger.Equals(trigger));
        }
        
        #endregion

        #region private support routines
		private StateConfigurationHelper<TState, TTrigger> PermitInternalSync(Func<bool> predicate,
            TTrigger trigger,
            TState resultingState, Action onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            if (FindTriggerRepresentation(trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger, currentStateRepresentation);

            rep.NextStateRepresentationPredicate = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = predicate;

            return this;
        }

        private StateConfigurationHelper<TState, TTrigger> PermitInternalSync<TArgument>(Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<TArgument> onEntryAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            if (FindTriggerRepresentation(trigger.Trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger.Trigger, currentStateRepresentation);
            rep.NextStateRepresentationPredicate = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = predicate;

            return this;
        }

        private StateConfigurationHelper<TState, TTrigger> PermitInternalAsync(Func<Task<bool>> predicate,
            TTrigger trigger,
            TState resultingState, Func<Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            if (FindTriggerRepresentation(trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger, currentStateRepresentation);
            rep.NextStateRepresentationPredicate = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= TransitionFlag.TriggerPredicateReturnsTask;
            rep.TransitionFlags |= TransitionFlag.TriggerActionReturnsTask;

            return this;
        }

        private StateConfigurationHelper<TState, TTrigger> PermitInternalAsync<TArgument>(
            Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            if (FindTriggerRepresentation(trigger.Trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger.Trigger, currentStateRepresentation);
            rep.NextStateRepresentationPredicate = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= TransitionFlag.TriggerPredicateReturnsTask;
            rep.TransitionFlags |= TransitionFlag.TriggerActionReturnsTask;

            return this;
        }

        private StateConfigurationHelper<TState, TTrigger> PermitInternalTriggerAsync(Func<bool> predicate,
            TTrigger trigger,
            TState resultingState, Func<Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            if (FindTriggerRepresentation(trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger, currentStateRepresentation);
            rep.NextStateRepresentationPredicate = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= TransitionFlag.TriggerActionReturnsTask;

            return this;
        }

        private StateConfigurationHelper<TState, TTrigger> PermitInternalTriggerAsync<TArgument>(
            Func<bool> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Func<TArgument, Task> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            if (FindTriggerRepresentation(trigger.Trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger.Trigger, currentStateRepresentation);
            rep.NextStateRepresentationPredicate = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= TransitionFlag.TriggerActionReturnsTask;

            return this;
        }

        private StateConfigurationHelper<TState, TTrigger> PermitInternalPredicateAsync(
            Func<Task<bool>> predicate,
            TTrigger trigger,
            TState resultingState, Action onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            if (FindTriggerRepresentation(trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger, currentStateRepresentation);
            rep.NextStateRepresentationPredicate = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= TransitionFlag.TriggerPredicateReturnsTask;

            return this;
        }

        private StateConfigurationHelper<TState, TTrigger> PermitInternalPredicateAsync<TArgument>(
            Func<Task<bool>> predicate,
            ParameterizedTrigger<TTrigger, TArgument> trigger,
            TState resultingState, Action<TArgument> onEntryAsyncAction)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);
            Contract.Requires<ArgumentNullException>(resultingState != null);

            if (FindTriggerRepresentation(trigger.Trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger.Trigger, currentStateRepresentation);
            rep.NextStateRepresentationPredicate = FindOrCreateStateRepresentation(resultingState, config);
            rep.OnTriggerAction = onEntryAsyncAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= TransitionFlag.TriggerPredicateReturnsTask;

            return this;
        }

        private StateConfigurationHelper<TState, TTrigger> PermitDynamicInternal(
                                                        Func<bool> predicate,
                                                        TTrigger trigger,
                                                        Func<TState> targetStatePredicate,
                                                        Action onEntryAction)
        {
            if (FindTriggerRepresentation(trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();
            var rep = CreateTriggerRepresentation(trigger, currentStateRepresentation);
            rep.NextStateRepresentationPredicate = targetStatePredicate;
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= TransitionFlag.DynamicState;

            return this;
        }

        private StateConfigurationHelper<TState, TTrigger> PermitDynamicInternal<TArgument>(
                                                        Func<bool> predicate,
                                                        ParameterizedTrigger<TTrigger, TArgument> trigger,
                                                        Func<TState> targetStatePredicate,
                                                        Action<TArgument> onEntryAction)
        {
            if (FindTriggerRepresentation(trigger.Trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();
            var rep = CreateTriggerRepresentation(trigger.Trigger, currentStateRepresentation);
            rep.NextStateRepresentationPredicate = targetStatePredicate;
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= TransitionFlag.DynamicState;

            return this;
        }

        private StateConfigurationHelper<TState, TTrigger> PermitDynamicInternalPredicateAsync(
                                                                Func<Task<bool>> predicate,
                                                                TTrigger trigger,
                                                                Func<TState> targetStatePredicate,
                                                                Action onEntryAction)
        {
            if (FindTriggerRepresentation(trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();
            var rep = CreateTriggerRepresentation(trigger, currentStateRepresentation);
            rep.NextStateRepresentationPredicate = targetStatePredicate;
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= TransitionFlag.DynamicState | TransitionFlag.TriggerPredicateReturnsTask;

            return this;
        }

        private StateConfigurationHelper<TState, TTrigger> PermitDynamicInternalPredicateAsync<TArgument>(
                                                                Func<Task<bool>> predicate,
                                                                ParameterizedTrigger<TTrigger, TArgument> trigger,
                                                                Func<TState> targetStatePredicate,
                                                                Action<TArgument> onEntryAction)
        {
            if (FindTriggerRepresentation(trigger.Trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();
            var rep = CreateTriggerRepresentation(trigger.Trigger, currentStateRepresentation);
            rep.NextStateRepresentationPredicate = targetStatePredicate;
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= TransitionFlag.DynamicState | TransitionFlag.TriggerPredicateReturnsTask;

            return this;
        }

        private StateConfigurationHelper<TState, TTrigger> PermitDynamicInternalTriggerAsync(
                                                                Func<bool> predicate,
                                                                TTrigger trigger,
                                                                Func<TState> targetStatePredicate,
                                                                Func<Task> onEntryAction)
        {
            if (FindTriggerRepresentation(trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();
            var rep = CreateTriggerRepresentation(trigger, currentStateRepresentation);
            rep.NextStateRepresentationPredicate = targetStatePredicate;
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= TransitionFlag.TriggerActionReturnsTask | TransitionFlag.DynamicState;

            return this;
        }

        private StateConfigurationHelper<TState, TTrigger> PermitDynamicInternalTriggerAsync<TArgument>(
                                                                Func<bool> predicate,
                                                                ParameterizedTrigger<TTrigger, TArgument> trigger,
                                                                Func<TState> targetStatePredicate,
                                                                Func<TArgument, Task> onEntryAction)
        {
            if (FindTriggerRepresentation(trigger.Trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();
            var rep = CreateTriggerRepresentation(trigger.Trigger, currentStateRepresentation);
            rep.NextStateRepresentationPredicate = targetStatePredicate;
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= TransitionFlag.TriggerActionReturnsTask | TransitionFlag.DynamicState;

            return this;
        }

        private StateConfigurationHelper<TState, TTrigger> PermitDynamicInternalPredicateAsyncTriggerAsync(
                                                                Func<Task<bool>> predicate,
                                                                TTrigger trigger,
                                                                Func<TState> targetStatePredicate,
                                                                Func<Task> onEntryAction)
        {
            if (FindTriggerRepresentation(trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();
            var rep = CreateTriggerRepresentation(trigger, currentStateRepresentation);
            rep.NextStateRepresentationPredicate = targetStatePredicate;
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= TransitionFlag.TriggerActionReturnsTask | TransitionFlag.DynamicState | TransitionFlag.TriggerPredicateReturnsTask;

            return this;
        }

        private StateConfigurationHelper<TState, TTrigger> PermitDynamicInternalPredicateAsyncTriggerAsync<TArgument>(
                                                                Func<Task<bool>> predicate,
                                                                ParameterizedTrigger<TTrigger, TArgument> trigger,
                                                                Func<TState> targetStatePredicate,
                                                                Func<TArgument, Task> onEntryAction)
        {
            if (FindTriggerRepresentation(trigger.Trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();
            var rep = CreateTriggerRepresentation(trigger.Trigger, currentStateRepresentation);
            rep.NextStateRepresentationPredicate = targetStatePredicate;
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= TransitionFlag.TriggerActionReturnsTask | TransitionFlag.DynamicState | TransitionFlag.TriggerPredicateReturnsTask;

            return this;
        }

        private StateConfigurationHelper<TState, TTrigger> PermitDynamicAsyncInternal(
                                                                Func<bool> predicate,
                                                                TTrigger trigger,
                                                                Func<Task<TState>> targetStatePredicate,
                                                                Action onEntryAction)
        {
            if (FindTriggerRepresentation(trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();
            var rep = CreateTriggerRepresentation(trigger, currentStateRepresentation);
            rep.NextStateRepresentationPredicate = targetStatePredicate;
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= TransitionFlag.DynamicStateReturnsTask;

            return this;
        }

        private StateConfigurationHelper<TState, TTrigger> PermitDynamicAsyncInternal<TArgument>(
                                                                Func<bool> predicate,
                                                                ParameterizedTrigger<TTrigger, TArgument> trigger,
                                                                Func<Task<TState>> targetStatePredicate,
                                                                Action<TArgument> onEntryAction)
        {
            if (FindTriggerRepresentation(trigger.Trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();
            var rep = CreateTriggerRepresentation(trigger.Trigger, currentStateRepresentation);
            rep.NextStateRepresentationPredicate = targetStatePredicate;
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= TransitionFlag.DynamicStateReturnsTask;

            return this;
        }

        private StateConfigurationHelper<TState, TTrigger> PermitDynamicAsyncInternalPredicateAsync(
                                                                Func<Task<bool>> predicate,
                                                                TTrigger trigger,
                                                                Func<Task<TState>> targetStatePredicate,
                                                                Action onEntryAction)
        {
            if (FindTriggerRepresentation(trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();
            var rep = CreateTriggerRepresentation(trigger, currentStateRepresentation);
            rep.NextStateRepresentationPredicate = targetStatePredicate;
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= TransitionFlag.DynamicStateReturnsTask | TransitionFlag.TriggerPredicateReturnsTask;

            return this;
        }

        private StateConfigurationHelper<TState, TTrigger> PermitDynamicAsyncInternalPredicateAsync<TArgument>(
                                                                Func<Task<bool>> predicate,
                                                                ParameterizedTrigger<TTrigger, TArgument> trigger,
                                                                Func<Task<TState>> targetStatePredicate,
                                                                Action<TArgument> onEntryAction)
        {
            if (FindTriggerRepresentation(trigger.Trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();
            var rep = CreateTriggerRepresentation(trigger.Trigger, currentStateRepresentation);
            rep.NextStateRepresentationPredicate = targetStatePredicate;
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= TransitionFlag.DynamicStateReturnsTask | TransitionFlag.TriggerPredicateReturnsTask;

            return this;
        }

        private StateConfigurationHelper<TState, TTrigger> PermitDynamicAsyncInternalTriggerAsync(
                                                                Func<bool> predicate,
                                                                TTrigger trigger,
                                                                Func<Task<TState>> targetStatePredicate,
                                                                Func<Task> onEntryAction)
        {
            if (FindTriggerRepresentation(trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();
            var rep = CreateTriggerRepresentation(trigger, currentStateRepresentation);
            rep.NextStateRepresentationPredicate = targetStatePredicate;
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= TransitionFlag.TriggerActionReturnsTask | TransitionFlag.DynamicStateReturnsTask;

            return this;
        }

        private StateConfigurationHelper<TState, TTrigger> PermitDynamicAsyncInternalTriggerAsync<TArgument>(
                                                                Func<bool> predicate,
                                                                ParameterizedTrigger<TTrigger, TArgument> trigger,
                                                                Func<Task<TState>> targetStatePredicate,
                                                                Func<TArgument, Task> onEntryAction)
        {
            if (FindTriggerRepresentation(trigger.Trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();
            var rep = CreateTriggerRepresentation(trigger.Trigger, currentStateRepresentation);
            rep.NextStateRepresentationPredicate = targetStatePredicate;
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= TransitionFlag.TriggerActionReturnsTask | TransitionFlag.DynamicStateReturnsTask;

            return this;
        }

        private StateConfigurationHelper<TState, TTrigger> PermitDynamicAsyncInternalPredicateAsyncTriggerAsync(
                                                                Func<Task<bool>> predicate,
                                                                TTrigger trigger,
                                                                Func<Task<TState>> targetStatePredicate,
                                                                Func<Task> onEntryAction)
        {
            if (FindTriggerRepresentation(trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();
            var rep = CreateTriggerRepresentation(trigger, currentStateRepresentation);
            rep.NextStateRepresentationPredicate = targetStatePredicate;
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= TransitionFlag.TriggerActionReturnsTask
                                | TransitionFlag.DynamicStateReturnsTask
                                | TransitionFlag.TriggerPredicateReturnsTask;

            return this;
        }

        private StateConfigurationHelper<TState, TTrigger> PermitDynamicAsyncInternalPredicateAsyncTriggerAsync<TArgument>(
                                                                Func<Task<bool>> predicate,
                                                                ParameterizedTrigger<TTrigger, TArgument> trigger,
                                                                Func<Task<TState>> targetStatePredicate,
                                                                Func<TArgument, Task> onEntryAction)
        {
            if (FindTriggerRepresentation(trigger.Trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();
            var rep = CreateTriggerRepresentation(trigger.Trigger, currentStateRepresentation);
            rep.NextStateRepresentationPredicate = targetStatePredicate;
            rep.OnTriggerAction = onEntryAction;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= TransitionFlag.TriggerActionReturnsTask
                                | TransitionFlag.DynamicStateReturnsTask
                                | TransitionFlag.TriggerPredicateReturnsTask;

            return this;
        }

        private StateConfigurationHelper<TState, TTrigger> IgnoreInternal(Func<bool> predicate,
            TTrigger trigger)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);

            if (FindTriggerRepresentation(trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger, currentStateRepresentation);
            rep.NextStateRepresentationPredicate = null;
            rep.ConditionalTriggerPredicate = predicate;

            return this;
        }

        private StateConfigurationHelper<TState, TTrigger> IgnoreInternalPredicateAsync(
            Func<Task<bool>> predicate,
            TTrigger trigger)
        {
            Contract.Requires<ArgumentNullException>(trigger != null);

            if (FindTriggerRepresentation(trigger, currentStateRepresentation) != null)
                ExceptionHelper.ThrowExclusiveOperation();

            var rep = CreateTriggerRepresentation(trigger, currentStateRepresentation);
            rep.NextStateRepresentationPredicate = null;
            rep.ConditionalTriggerPredicate = predicate;
            rep.TransitionFlags |= TransitionFlag.TriggerPredicateReturnsTask;

            return this;
        }
 
	    #endregion 
    }
}
