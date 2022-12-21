﻿using _Scripts.Circuit_Simulator;
using _Scripts.Enums;
using NaughtyAttributes;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;

namespace _Scripts.Elements.Components
{
    public class SourceComponent : CircuitComponent
    {
        [ShowNonSerializedField] private bool _isFirstSource;

        private protected override Entity EntityComponent =>
            new VoltageSource(ElementName, GetInNode(), GetOutNode(), elementsValue);

        // If this is the first voltage source we are adding, make sure one of 
        // the ends is specified as ground, or "0" Volt point of reference.
        public override string GetInNode() => _isFirstSource ? "0" : base.GetInNode();
        
        public void SetAsFirstSource()
        {
            _isFirstSource = true;
        }

        public override void ClearValues()
        {
            base.ClearValues();
            _isFirstSource = false;
        }

        public override void UpdateExports(OP op)
        {
            base.UpdateExports(op);
            Exports.Add(item: new ExportData(ElementsValue.V,
                new RealPropertyExport(op, ElementName, "v")));
            Exports.Add(item: new ExportData(ElementsValue.A,
                new RealPropertyExport(op, ElementName, "i")));
        }
    }
}