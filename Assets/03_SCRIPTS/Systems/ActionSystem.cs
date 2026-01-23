using System;
using System.Collections;
using System.Collections.Generic;
using MADP.Models.UnitActions;
using MADP.Ultility;
using UnityEngine;

namespace MADP.Systems
{
    public enum UnitActionEventType
    {
        PRE, POST
    }
    
    public class ActionSystem : Singleton<ActionSystem>
    {
        private List<BaseUnitAction> reactions = null;
        public bool IsPerforming { get; private set; } = false;
        
        private static Dictionary<Type, List<Action<BaseUnitAction>>> preSubs = new ();
        private static Dictionary<Type, List<Action<BaseUnitAction>>> postSubs = new ();
        private static Dictionary<Type, Func<BaseUnitAction, IEnumerator>> performers = new ();
        
        #region --- MAIN METHODS ---
        public void Perform(BaseUnitAction action, Action OnPerformFinished = null)
        {
            if (IsPerforming) return;
            IsPerforming = true;
            StartCoroutine(Flow(action, () =>
            {
                IsPerforming = false;
                OnPerformFinished?.Invoke();
            }));
        }

        public void AddReaction(BaseUnitAction action)
        {
            reactions?.Add(action);
        }
        
        public static void AttachPerformer<T>(Func<T, IEnumerator> performer) where T : BaseUnitAction
        {
            Type type = typeof(T);
            IEnumerator wrappedPerformer(BaseUnitAction action) => performer((T)action);
            
            if (performers.ContainsKey(type)) 
                performers[type] = wrappedPerformer;
            else 
                performers.Add(type, wrappedPerformer);
        }

        public static void DetachPerformer<T>() where T : BaseUnitAction
        {
            Type type = typeof(T);
            if (performers.ContainsKey(type)) 
                performers.Remove(type);
        }

        public static void SubscribeReaction<T>(Action<T> reaction, UnitActionEventType timing) where T : BaseUnitAction
        {
            var subs = timing == UnitActionEventType.PRE ? preSubs : postSubs;
            void wrappedReaction(BaseUnitAction action) => reaction((T)action);
            
            if (!subs.ContainsKey(typeof(T))) 
                subs.Add(typeof(T), new());
            
            subs[typeof(T)].Add(wrappedReaction);
        }

        public static void UnsubscribeReaction<T>(Action<T> reaction, UnitActionEventType timing) where T : BaseUnitAction
        {
            var subs = timing == UnitActionEventType.PRE ? preSubs : postSubs;
            if (subs.ContainsKey(typeof(T)))
            {
                void wrappedReaction(BaseUnitAction action) => reaction((T)action);
                subs[typeof(T)].Remove(wrappedReaction);
            }
        }
        #endregion
        
        #region ---HELPER METHODS ---
        private IEnumerator Flow(BaseUnitAction action, Action OnFlowFinished = null)
        {
            reactions = action.PreActions;
            PerformSubscribers(action, preSubs);
            yield return PerformReactions();
            
            reactions = action.PerformActions;
            yield return PerformPerformer(action);
            yield return PerformReactions();
            
            reactions = action.PostActions;
            PerformSubscribers(action, postSubs);
            yield return PerformReactions();

            OnFlowFinished?.Invoke();
        }

        private IEnumerator PerformPerformer(BaseUnitAction action)
        {
            Type type = action.GetType();
            if (performers.ContainsKey(type))
            {
                yield return performers[type](action);
            }
        }
        
        private void PerformSubscribers(BaseUnitAction action, Dictionary<Type, List<Action<BaseUnitAction>>> subs)
        {
            Type type = action.GetType();
            if (subs.ContainsKey(type))
            {
                foreach (var sub in subs[type]) 
                    sub(action);
            }
        }   

        private IEnumerator PerformReactions()
        {
            foreach (var reaction in reactions)
            {
                yield return Flow(reaction);
            }
        }
        #endregion
    }
}

