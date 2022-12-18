﻿using System;
using System.Globalization;
using Elements;
using Enums;
using NaughtyAttributes;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using UnityEngine;

namespace _Scripts.Elements
{
    public class CircuitElement : ElementWithMotion
    {
        [Space] [SerializeField] private ElementType elementType;
        [SerializeField] private float elementsValue;

        [ShowNonSerializedField, Foldout("Info"), Label("In")]
        private string _infoInNode;
        [ShowNonSerializedField, Foldout("Info"), Label("Out")]
        private string _infoOutNode;

        [field: ShowNonSerializedField]
        [field: Foldout("Info")]
        [field: Label("Out")]
        public string ElementName { get; private set; }

        private ElementWithMotion _elementFromThis;
        private ElementWithMotion _elementToThis;
        private LineRenderer _lineRenderer;

        public bool IsUsed { private set; get; }
        
        public ElementType ElementType => elementType;

        public bool IsFirstSource { get; private set; }

        public string InNode
        {
            get
            {
                if (IsFirstSource) return _infoInNode = "0";
                return _infoInNode = _elementToThis != null ? _elementToThis.OutNode : null;
            }
        }

        private string _possibleOutNode;
        public override string OutNode
        {
            // if next element is element we create node between them
            // if next element is node element we use its node
            get
            {
                if (_elementFromThis == null)
                {
                    return _infoOutNode = null;
                }
                if (_elementFromThis.TryGetComponent(out NodeElement nodeElement))
                {
                    return _infoOutNode = nodeElement.OutNode;
                }
                if (_elementFromThis.TryGetComponent(out CircuitElement circuitElement))
                {
                    if (circuitElement.IsFirstSource)
                    {
                        return _infoOutNode = "0";
                    }
                    return _infoOutNode = _possibleOutNode;
                }
                return _infoOutNode = null;
            }
        }
        
        
        private protected override void Start()
        {
            ElementName = CircuitSimulator.CreateElement(elementType.ToString());
            _possibleOutNode = CircuitSimulator.CreateNode();

            base.Start();
            CircuitSimulator.Instance.AddElement(this);
        }

        private protected override void Update()
        {
            base.Update();
            if (_elementFromThis)
            {
                DrawLine(_lineRenderer, _elementFromThis);
            }
        }

        private void OnDestroy()
        {
            CircuitSimulator.Instance.RemoveElement(this);
            _elementToThis.AddElementsFromThis(this);
        }
        
        #region wire creation
        public override void AddElementsFromThis(ElementWithMotion circuitElement)
        {
            if (_elementFromThis == circuitElement)
            {
                _elementFromThis = null;
                Destroy(_lineRenderer);
                return;
            }
            if (_elementFromThis == null)
            {
                _lineRenderer = Instantiate(wire, outputPoint);
            }

            _elementFromThis = circuitElement;
        }

        public override void AddElementsToThis(ElementWithMotion circuitElement)
        {
            if (_elementToThis == circuitElement)
            {
                _elementToThis.AddElementsFromThis(this);
                _elementToThis = null;
                return;
            }
            if (_elementToThis != null)
            {
                _elementToThis.AddElementsFromThis(this);
            }
            _elementToThis = circuitElement;
        }
        #endregion
        
        public Entity GetElement(bool isFirstSource = false)
        {
            IsFirstSource = isFirstSource && elementType == ElementType.VoltageSource;
            if (OutNode == null || InNode == null)
            {
                return null;
            }
            IsUsed = true;

            Entity element = elementType switch
            {
                ElementType.VoltageSource => new VoltageSource(ElementName, InNode, OutNode, elementsValue),
                ElementType.Resistor => new Resistor(ElementName, InNode, OutNode, elementsValue),
                _ => throw new ArgumentOutOfRangeException()
            };

            return element;
        }

        
        #region Data update
        public void ClearValues()
        {
            elementData.ClearValues();
            IsUsed = false;
        }

        public override string UpdateValue(string value)
        {
            try
            {
                if (value == "")
                {
                    elementsValue = 0;
                }
                else
                {
                    elementsValue = float.Parse(value);    
                }

                return value;
            }
            catch
            {
                return GetValue();
            }
        }

        public override string GetValue()
        {
            return elementsValue.ToString(CultureInfo.InvariantCulture);
        }

        
        #endregion
    }
}