using System;
using System.Collections.Generic;
using UnityEngine;

namespace Investigation.Behaviors
{
    public abstract class VisualBindingsBase<T> : ScriptableObject where T : class
    {
        protected readonly Dictionary<Type, GameObject> prefabBindings = new Dictionary<Type, GameObject>();
        protected readonly Dictionary<Type, string> timelineAnnotations = new Dictionary<Type, string>();

        protected void AddBinding(Type elementType, GameObject prefab, string timelineAnnotation)
        {
            prefabBindings.Add(elementType, prefab);
            timelineAnnotations.Add(elementType, timelineAnnotation);
        }

        public abstract void Initialize();

        public GameObject GetPrefabFor(T element)
        {
            Type cardType = element.GetType();
            return prefabBindings[cardType];
        }

        public string GetTimelineAnnotationFor(Type modifierType)
        {
            return timelineAnnotations[modifierType];
        }
    }
}