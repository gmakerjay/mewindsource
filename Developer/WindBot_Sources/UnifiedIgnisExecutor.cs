using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using WindBot.Game;
using WindBot.Game.AI;
using YGOSharp.OCGWrapper.Enums;

namespace ProjectIgnisAI
{
    // Register the deck name and its corresponding .ydk filename
    [Deck("UnifiedIgnis", "AI_CustomIgnis")]
    public class UnifiedIgnisExecutor : BaseCustomExecutor
    {
        public UnifiedIgnisExecutor(GameAI ai, Duel duel) : base(ai, duel)
        {
        }
    }

    // NOTE: The following deck-specific subclasses are purposefully empty stubs.
    // They inherit from UnifiedIgnisExecutor and rely entirely on the dynamic, weight-based
    // registry mapping loaded from JSON config files (e.g. cards_registry_*.json) inside BaseCustomExecutor.
    // Overriding is only necessary if custom complex combo plans need to be coded manually.

    [Deck("2026_AzaYummy", "2026_AzaYummy")]
    public class AzaYummyExecutor : UnifiedIgnisExecutor
    {
        public AzaYummyExecutor(GameAI ai, Duel duel) : base(ai, duel) {}
    }

    [Deck("2026_BrElfnote", "2026_BrElfnote")]
    public class BrElfnoteExecutor : UnifiedIgnisExecutor
    {
        public BrElfnoteExecutor(GameAI ai, Duel duel) : base(ai, duel) {}
    }

    [Deck("2026_DarkTime", "2026_DarkTime")]
    public class DarkTimeExecutor : UnifiedIgnisExecutor
    {
        public DarkTimeExecutor(GameAI ai, Duel duel) : base(ai, duel) {}
    }

    [Deck("2026_EvilTwin", "2026_EvilTwin")]
    public class EvilTwinExecutor : UnifiedIgnisExecutor
    {
        public EvilTwinExecutor(GameAI ai, Duel duel) : base(ai, duel) {}
    }

    [Deck("2026_EyeInside", "2026_EyeInside")]
    public class EyeInsideExecutor : UnifiedIgnisExecutor
    {
        public EyeInsideExecutor(GameAI ai, Duel duel) : base(ai, duel) {}
    }

    [Deck("2026_Hecahand", "2026_Hecahand")]
    public class HecahandExecutor : UnifiedIgnisExecutor
    {
        public HecahandExecutor(GameAI ai, Duel duel) : base(ai, duel) {}
    }

    [Deck("2026_Goldlord", "2026_Goldlord")]
    public class GoldlordExecutor : UnifiedIgnisExecutor
    {
        public GoldlordExecutor(GameAI ai, Duel duel) : base(ai, duel) {}
    }

    // 2026_Invoke executor is defined in a separate file (InvokeExecutor.cs)

    [Deck("2026_Kwtune", "2026_Kwtune")]
    public class KwtuneExecutor : UnifiedIgnisExecutor
    {
        public KwtuneExecutor(GameAI ai, Duel duel) : base(ai, duel) {}
    }

    [Deck("2026_Labrynth", "2026_Labrynth")]
    public class LabrynthExecutor : UnifiedIgnisExecutor
    {
        public LabrynthExecutor(GameAI ai, Duel duel) : base(ai, duel) {}
    }


}
