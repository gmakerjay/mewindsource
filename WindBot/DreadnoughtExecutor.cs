using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using WindBot.Game;
using WindBot.Game.AI;
using YGOSharp.OCGWrapper.Enums;

namespace ProjectIgnisAI
{
    [Deck("2026_Dreadnought", "2026_Dreadnought")]
    public class DreadnoughtExecutor : UnifiedIgnisExecutor
    {
        private const int HintMsg_Discard = 501;
        private const int HintMsg_AddToHand = 506;
        private const int HintMsg_SpSummon = 509;
        private const int HintMsg_FusionMaterial = 511;

        public DreadnoughtExecutor(GameAI ai, Duel duel) : base(ai, duel)
        {
            // Clear default weight-based dynamic registry to enforce strict, prioritized sequencing rules (BaseRules)
            Executors.Clear();

            // 1. Hand traps, interruptions, and negates (high priority response handlers)
            AddExecutor(ExecutorType.Activate, 14558127, () => OnCardAction(14558127, ExecutorType.Activate, AshBlossomEffect)); // Ash Blossom
            AddExecutor(ExecutorType.Activate, 10045474, () => OnCardAction(10045474, ExecutorType.Activate, ImpermanenceEffect)); // Infinite Impermanence
            AddExecutor(ExecutorType.Activate, 24224830, () => OnCardAction(24224830, ExecutorType.Activate, CalledByGraveEffect)); // Called by the Grave (ID 1)
            AddExecutor(ExecutorType.Activate, 24224830, () => OnCardAction(24224830, ExecutorType.Activate, CalledByGraveEffect)); // Called by the Grave (ID 2)
            AddExecutor(ExecutorType.Activate, 42141493, () => OnCardAction(42141493, ExecutorType.Activate, FuwalosEffect)); // Mulcharmy Fuwalos
            AddExecutor(ExecutorType.Activate, 94145021, () => OnCardAction(94145021, ExecutorType.Activate, DrollEffect)); // Droll & Lock Bird
            AddExecutor(ExecutorType.Activate, 6325660, () => OnCardAction(6325660, ExecutorType.Activate, DominusSparkEffect)); // Dominus Spark
            AddExecutor(ExecutorType.Activate, 78114463, () => OnCardAction(78114463, ExecutorType.Activate, SolemnReportEffect)); // Solemn Report

            // 2. Search & Field Spells
            AddExecutor(ExecutorType.Activate, 73628505, () => OnCardAction(73628505, ExecutorType.Activate, TerraformingEffect)); // Terraforming
            AddExecutor(ExecutorType.Activate, 101402062, () => OnCardAction(101402062, ExecutorType.Activate, DarkCityFieldEffect)); // Clock Tower Prison City - Dark City Field Activation
            AddExecutor(ExecutorType.Activate, 101402022, () => OnCardAction(101402022, ExecutorType.Activate, DoomLiegeEffect)); // Doom Liege (Banish/Search)
            AddExecutor(ExecutorType.Activate, 40237839, () => OnCardAction(40237839, ExecutorType.Activate, SabatielEffect)); // Sabatiel (Search Poly/Fusion)

            // 3. Extenders & Special Summons (Hand/GY)
            AddExecutor(ExecutorType.SpSummon, 101402023, () => OnCardAction(101402023, ExecutorType.SpSummon, DreadnoughtServantSpSummon)); // Dreadnought Servant hand SS
            AddExecutor(ExecutorType.Activate, 101402023, () => OnCardAction(101402023, ExecutorType.Activate, DreadnoughtServantEffect)); // Dreadnought Servant (Destroy/Search or GY spin)
            AddExecutor(ExecutorType.SpSummon, 10808715, () => OnCardAction(10808715, ExecutorType.SpSummon, DuskCrowSpSummon)); // Dusk Crow SS
            AddExecutor(ExecutorType.Activate, 10808715, () => OnCardAction(10808715, ExecutorType.Activate, DuskCrowEffect)); // Dusk Crow search
            AddExecutor(ExecutorType.Activate, 58288218, () => OnCardAction(58288218, ExecutorType.Activate, MaskedHeroFurnaceEffect)); // Furnace search
            AddExecutor(ExecutorType.SpSummon, 58288218, () => OnCardAction(58288218, ExecutorType.SpSummon, MaskedHeroFurnaceSpSummon)); // Furnace SS from GY
            AddExecutor(ExecutorType.Activate, 66206748, () => OnCardAction(66206748, ExecutorType.Activate, MaskedHeroFountainEffect)); // Fountain SS/Set Mask Change
            AddExecutor(ExecutorType.SpSummon, 101402021, () => OnCardAction(101402021, ExecutorType.SpSummon, DeathDogmaSpSummon)); // Death Dogma SS
            AddExecutor(ExecutorType.Activate, 101402021, () => OnCardAction(101402021, ExecutorType.Activate, DeathDogmaEffect)); // Death Dogma (Burn or Quick Fusion)
            AddExecutor(ExecutorType.SpSummon, 83965310, () => OnCardAction(83965310, ExecutorType.SpSummon, PlasmaSpSummon)); // Plasma SS
            AddExecutor(ExecutorType.Activate, 83965310, () => OnCardAction(83965310, ExecutorType.Activate, PlasmaEffect)); // Plasma Absorb

            // 4. Normal Summons
            AddExecutor(ExecutorType.Summon, 27780618, () => OnCardAction(27780618, ExecutorType.Summon, VyonSummon)); // Vision HERO Vyon
            AddExecutor(ExecutorType.Summon, 101402022, () => OnCardAction(101402022, ExecutorType.Summon, DoomLiegeSummon)); // Doom Liege
            AddExecutor(ExecutorType.Summon, 50720316, () => OnCardAction(50720316, ExecutorType.Summon, ShadowMistSummon)); // Shadow Mist
            AddExecutor(ExecutorType.Summon, 101402023, () => OnCardAction(101402023, ExecutorType.Summon, DreadnoughtServantSummon)); // Servant

            // 5. Summon Trigger Effects & Searches
            AddExecutor(ExecutorType.Activate, 27780618, () => OnCardAction(27780618, ExecutorType.Activate, VyonEffect)); // Vyon Send/Search Poly
            AddExecutor(ExecutorType.Activate, 50720316, () => OnCardAction(50720316, ExecutorType.Activate, ShadowMistEffect)); // Shadow Mist Search

            // 6. Fusion & Spell Engines
            AddExecutor(ExecutorType.Activate, 52947044, () => OnCardAction(52947044, ExecutorType.Activate, FusionDestinyEffect)); // Fusion Destiny
            AddExecutor(ExecutorType.Activate, 24094653, () => OnCardAction(24094653, ExecutorType.Activate, PolymerizationEffect)); // Polymerization
            AddExecutor(ExecutorType.Activate, 21143940, () => OnCardAction(21143940, ExecutorType.Activate, MaskChangeEffect)); // Mask Change
            AddExecutor(ExecutorType.Activate, 48130397, () => OnCardAction(48130397, ExecutorType.Activate, SuperPolymerizationEffect)); // Super Poly
            AddExecutor(ExecutorType.Activate, 100456010, () => OnCardAction(100456010, ExecutorType.Activate, DBurstEffect)); // D-Burst (Destroy/SS or GY Double Attack)

            // 7. Extra Deck Proc & Boss Triggers
            AddExecutor(ExecutorType.SpSummon, 101402037, () => OnCardAction(101402037, ExecutorType.SpSummon, DreadnoughtSpSummon)); // Dreadnought Alt Summon Proc
            AddExecutor(ExecutorType.Activate, 101402037, () => OnCardAction(101402037, ExecutorType.Activate, DreadnoughtEffect)); // Dreadnought Search
            AddExecutor(ExecutorType.Activate, 60461804, () => OnCardAction(60461804, ExecutorType.Activate, DPEEffect)); // DPE Destroy/Rebirth
            AddExecutor(ExecutorType.Activate, 90579153, () => OnCardAction(90579153, ExecutorType.Activate, DystopiaEffect)); // Dystopia Destroy
            AddExecutor(ExecutorType.Activate, 23204029, () => OnCardAction(23204029, ExecutorType.Activate, ContrastHeroChaosEffect)); // Contrast HERO Chaos Negate
            AddExecutor(ExecutorType.Activate, 9411399, () => OnCardAction(9411399, ExecutorType.Activate, MaliciousEffect)); // Malicious Summon another copy
            AddExecutor(ExecutorType.Activate, 16605586, () => OnCardAction(16605586, ExecutorType.Activate, DenierEffect)); // Denier Recycle/SS

            // 8. Catch-All Fallbacks
            AddExecutor(ExecutorType.Activate, OnDefaultActivate);
            AddExecutor(ExecutorType.Summon, OnDefaultSummon);
            AddExecutor(ExecutorType.SpSummon, OnDefaultSpSummon);
            AddExecutor(ExecutorType.SpellSet, OnDefaultSpellSet);
            AddExecutor(ExecutorType.Repos, OnDefaultRepos);
            AddExecutor(ExecutorType.MonsterSet, OnDefaultMonsterSet);
        }

        // --- HELPER METHODS ---

        private bool HasInHand(int cardId)
        {
            if (Bot == null || Bot.Hand == null) return false;
            foreach (var c in Bot.Hand)
            {
                if (c != null && c.Id == cardId) return true;
            }
            return false;
        }

        private bool HasInGraveyard(int cardId)
        {
            if (Bot == null || Bot.Graveyard == null) return false;
            foreach (var c in Bot.Graveyard)
            {
                if (c != null && c.Id == cardId) return true;
            }
            return false;
        }

        private bool HasInSpellZone(int cardId)
        {
            if (Bot == null) return false;
            foreach (var c in Bot.GetSpells())
            {
                if (c != null && c.IsFaceup() && c.Id == cardId) return true;
            }
            return false;
        }

        private bool HasInMonsterZone(int cardId)
        {
            if (Bot == null) return false;
            foreach (var c in Bot.GetMonsters())
            {
                if (c != null && c.IsFaceup() && c.Id == cardId) return true;
            }
            return false;
        }

        private bool IsDestinyHero(ClientCard card)
        {
            if (card == null) return false;
            int id = card.Id;
            return id == 17132130  // Dogma
                || id == 101402021 // Death Dogma
                || id == 83965310  // Plasma
                || id == 40591390  // Dreadmaster
                || id == 9411399   // Malicious
                || id == 16605586  // Denier
                || id == 101402022 // Doom Liege
                || id == 101402023 // Dreadnought Servant
                || id == 101402037 // Dreadnought
                || id == 90579153  // Dystopia
                || id == 60461804  // DPE
                || id == 30757127; // Dangerous;
        }

        private bool IsHero(ClientCard card)
        {
            if (card == null) return false;
            if (IsDestinyHero(card)) return true;
            int id = card.Id;
            return id == 10808715  // Dusk Crow
                || id == 58288218  // Furnace
                || id == 50720316  // Shadow Mist
                || id == 66206748  // Fountain
                || id == 27780618  // Vyon
                || id == 93657021  // Divine Wind
                || id == 69394324  // Anki
                || id == 23204029  // Contrast HERO Chaos
                || id == 46759931  // Trinity
                || id == 58481572  // Dark Law
                || id == 63813056  // Blast
                || id == 1948619   // Wonder Driver
                || id == 58004362  // Cross Crusader
                || id == 87758525  // Shining Flare Wingman
                || id == 13243124  // Flame Wingman
                || id == 54757758; // Acid;
        }

        private int GetGraveWarriorDarkCount()
        {
            if (Bot == null || Bot.Graveyard == null) return 0;
            int count = 0;
            foreach (var card in Bot.Graveyard)
            {
                if (card != null && card.IsMonster() && (card.Race == (int)CardRace.Warrior || card.Attribute == (int)CardAttribute.Dark))
                {
                    count++;
                }
            }
            return count;
        }

        private int GetGraveHeroCount()
        {
            if (Bot == null || Bot.Graveyard == null) return 0;
            int count = 0;
            foreach (var card in Bot.Graveyard)
            {
                if (card != null && IsHero(card))
                {
                    count++;
                }
            }
            return count;
        }

        // --- SPECIFIC CARD RULE METHODS ---

        private bool AlreadyResponded()
        {
            if (Duel.CurrentChain.Count == 0) return false;
            int lastOpponentIndex = -1;
            for (int i = Duel.CurrentChain.Count - 1; i >= 0; i--)
            {
                if (Duel.CurrentChain[i].Controller == 1)
                {
                    lastOpponentIndex = i;
                    break;
                }
            }
            if (lastOpponentIndex == -1) return false;
            for (int i = lastOpponentIndex + 1; i < Duel.CurrentChain.Count; i++)
            {
                if (Duel.CurrentChain[i].Controller == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private bool AshBlossomEffect()
        {
            if (Duel.Player == 0) return false;
            if (AlreadyResponded()) return false;
            ClientCard lastChainCard = Util.GetLastChainCard();
            return lastChainCard != null && lastChainCard.Controller == 1 && !lastChainCard.IsDisabled();
        }

        private bool ImpermanenceEffect()
        {
            if (Duel.Player == 0) return false;
            if (AlreadyResponded()) return false;
            ClientCard lastChainCard = Util.GetLastChainCard();
            if (lastChainCard != null)
            {
                if (lastChainCard.Controller != 1 || lastChainCard.IsDisabled()) return false;
            }
            foreach (var m in Enemy.GetMonsters())
            {
                if (m != null && m.IsFaceup() && !m.IsDisabled())
                    return true;
            }
            return false;
        }

        private bool CalledByGraveEffect()
        {
            if (AlreadyResponded()) return false;
            ClientCard lastChainCard = Util.GetLastChainCard();
            if (lastChainCard != null && lastChainCard.Controller == 1 && lastChainCard.IsMonster() && !lastChainCard.IsDisabled())
            {
                foreach (var c in Enemy.Graveyard)
                {
                    if (c != null && c.Id == lastChainCard.Id)
                    {
                        AI.SelectCard(c);
                        return true;
                    }
                }
            }
            return false;
        }

        private bool FuwalosEffect()
        {
            if (Duel.Player == 0) return false;
            if (AlreadyResponded()) return false;
            ClientCard lastChainCard = Util.GetLastChainCard();
            if (lastChainCard != null && lastChainCard.Controller != 1) return false;
            return true;
        }

        private bool DrollEffect()
        {
            if (Duel.Player == 0) return false;
            if (AlreadyResponded()) return false;
            ClientCard lastChainCard = Util.GetLastChainCard();
            if (lastChainCard != null && lastChainCard.Controller != 1) return false;
            return true;
        }

        private bool DominusSparkEffect()
        {
            if (Duel.Player == 0) return false;
            if (AlreadyResponded()) return false;
            ClientCard lastChainCard = Util.GetLastChainCard();
            if (lastChainCard != null && (lastChainCard.Controller != 1 || lastChainCard.IsDisabled())) return false;
            return true;
        }

        private bool SolemnReportEffect()
        {
            if (Bot.LifePoints <= 1500)
            {
                LogToTurn("Block Solemn Report: LP too low (" + Bot.LifePoints + ")");
                return false;
            }
            if (AlreadyResponded()) return false;
            ClientCard lastChainCard = Util.GetLastChainCard();
            if (lastChainCard == null) return false;
            if (lastChainCard.Controller != 1 || lastChainCard.IsDisabled()) return false;
            if (!lastChainCard.IsSpell() && !lastChainCard.IsTrap()) return false;
            return true;
        }

        private bool TerraformingEffect()
        {
            // Search Dark City if not already held/active
            return !HasInSpellZone(101402062) && !HasInHand(101402062);
        }

        private bool DarkCityFieldEffect()
        {
            if (Card.Location == CardLocation.Hand)
            {
                return !HasInSpellZone(101402062);
            }
            else if (Card.Location == CardLocation.SpellZone)
            {
                // Search effect of Field Spell
                return true;
            }
            return false;
        }

        private bool DoomLiegeEffect()
        {
            if (Card.Location == CardLocation.MonsterZone)
            {
                // Trigger Summon effect: banish opponent monster
                if (Duel.LastChainPlayer != 0)
                {
                    return Enemy.GetMonsterCount() > 0;
                }
                // Ignition Search: search Dark City
                return !HasInHand(101402062) && !HasInSpellZone(101402062);
            }
            return false;
        }

        private bool SabatielEffect()
        {
            if (Card.Location == CardLocation.Hand)
            {
                // Search Poly/Fusion if we don't have them in hand
                return !HasInHand(24094653) && !HasInHand(52947044);
            }
            return false;
        }

        private bool DreadnoughtServantSpSummon()
        {
            if (Card.Location != CardLocation.Hand) return false;
            // SS if we control D-HERO or active Field Spell
            bool hasDHero = false;
            foreach (var m in Bot.GetMonsters())
            {
                if (m != null && m.IsFaceup() && IsDestinyHero(m))
                {
                    hasDHero = true;
                    break;
                }
            }
            bool hasField = false;
            foreach (var s in Bot.GetSpells())
            {
                if (s != null && s.IsFaceup() && s.HasType(CardType.Field))
                {
                    hasField = true;
                    break;
                }
            }
            return hasDHero || hasField;
        }

        private bool DreadnoughtServantEffect()
        {
            if (Card.Location == CardLocation.MonsterZone)
            {
                // Field Effect: destroy 1 card to search Poly
                bool hasTarget = false;
                foreach (var s in Bot.GetSpells())
                {
                    if (s != null && s.IsFaceup())
                    {
                        hasTarget = true;
                        break;
                    }
                }
                foreach (var m in Bot.GetMonsters())
                {
                    if (m != null && m.IsFaceup() && m != Card)
                    {
                        hasTarget = true;
                        break;
                    }
                }
                return hasTarget && !HasInHand(24094653) && !HasInGraveyard(24094653);
            }
            else if (Card.Location == CardLocation.Grave)
            {
                // GY Effect: spin opponent card
                return Enemy.GetMonsterCount() > 0 || Enemy.GetSpellCount() > 0;
            }
            return false;
        }

        private bool DuskCrowSpSummon()
        {
            if (Card.Location != CardLocation.Hand) return false;
            return GetGraveHeroCount() >= 1;
        }

        private bool DuskCrowEffect()
        {
            return Card.Location == CardLocation.MonsterZone;
        }

        private bool MaskedHeroFurnaceEffect()
        {
            if (Card.Location == CardLocation.Hand)
            {
                return (!HasInHand(21143940) && !HasInHand(24094653)) && Bot.Hand.Count > 1;
            }
            return false;
        }

        private bool MaskedHeroFurnaceSpSummon()
        {
            return Card.Location == CardLocation.Hand || Card.Location == CardLocation.Grave;
        }

        private bool MaskedHeroFountainEffect()
        {
            if (Card.Location == CardLocation.Hand)
            {
                foreach (var c in Bot.Hand)
                {
                    if (c != null && c != Card && IsHero(c))
                        return true;
                }
            }
            else if (Card.Location == CardLocation.Grave)
            {
                return true;
            }
            return false;
        }

        private bool DeathDogmaSpSummon()
        {
            if (Card.Location != CardLocation.Hand && Card.Location != CardLocation.Grave) return false;
            return GetGraveWarriorDarkCount() >= 3;
        }

        private bool DeathDogmaEffect()
        {
            if (Card.Location == CardLocation.MonsterZone)
            {
                if (Duel.Phase == DuelPhase.Standby)
                {
                    return true;
                }
                if (Duel.Player == 1) // Quick Fusion during opponent's turn
                {
                    return true;
                }
            }
            return false;
        }

        private bool PlasmaSpSummon()
        {
            if (Card.Location != CardLocation.Hand) return false;
            return Bot.GetMonsterCount() >= 3;
        }

        private bool PlasmaEffect()
        {
            foreach (var m in Enemy.GetMonsters())
            {
                if (m != null && m.IsFaceup()) return true;
            }
            return false;
        }

        private bool VyonSummon()
        {
            return true;
        }

        private bool DoomLiegeSummon()
        {
            // Summon if we don't have Field Spell and want to search it
            return !HasInSpellZone(101402062) && !HasInHand(101402062);
        }

        private bool ShadowMistSummon()
        {
            return true;
        }

        private bool DreadnoughtServantSummon()
        {
            return true;
        }

        private bool VyonEffect()
        {
            if (Card.Location == CardLocation.MonsterZone)
            {
                if (Duel.LastChainPlayer == 0) // Search Poly ignition
                {
                    return !HasInHand(24094653);
                }
                return true;
            }
            return false;
        }

        private bool ShadowMistEffect()
        {
            return Card.Location == CardLocation.MonsterZone || Card.Location == CardLocation.Grave;
        }

        private bool FusionDestinyEffect()
        {
            return true;
        }

        private bool PolymerizationEffect()
        {
            return true;
        }

        private bool MaskChangeEffect()
        {
            if (Duel.Player == 1) return true;
            if (Duel.Phase == DuelPhase.Battle)
            {
                foreach (var m in Bot.GetMonsters())
                {
                    if (m != null && m.IsFaceup() && m.Attacked && IsHero(m))
                        return true;
                }
            }
            else if (Duel.Phase == DuelPhase.Main1 || Duel.Phase == DuelPhase.Main2)
            {
                foreach (var m in Bot.GetMonsters())
                {
                    if (m != null && m.IsFaceup() && IsHero(m) && m.Attribute == (int)CardAttribute.Dark)
                        return !HasInMonsterZone(58481572);
                }
            }
            return false;
        }

        private bool SuperPolymerizationEffect()
        {
            return true;
        }

        private bool DBurstEffect()
        {
            if (Card.Location == CardLocation.Hand)
            {
                bool hasSpell = false;
                foreach (var s in Bot.GetSpells())
                {
                    if (s != null && s.IsFaceup())
                    {
                        hasSpell = true;
                        break;
                    }
                }
                return hasSpell;
            }
            else if (Card.Location == CardLocation.Grave)
            {
                if (Duel.Phase != DuelPhase.Battle) return false;
                foreach (var m in Bot.GetMonsters())
                {
                    if (m != null && m.IsFaceup() && (m.Id == 17132130 || m.Id == 101402021))
                        return true;
                }
            }
            return false;
        }

        private bool DreadnoughtSpSummon()
        {
            foreach (var m in Bot.GetMonsters())
            {
                if (m != null && m.IsFaceup() && m.Id == 40591390) // Dreadmaster
                    return true;
            }
            return false;
        }

        private bool DreadnoughtEffect()
        {
            return Card.Location == CardLocation.MonsterZone;
        }

        private bool DPEEffect()
        {
            if (Card.Location == CardLocation.MonsterZone)
            {
                if (AlreadyResponded()) return false;
                if (Duel.Player == 1)
                {
                    return Enemy.GetMonsterCount() > 0 || Enemy.GetSpellCount() > 0;
                }
                else if (Duel.Player == 0)
                {
                    if (HasInSpellZone(101402062)) return true; // destroy Dark City
                    return Enemy.GetMonsterCount() > 0;
                }
            }
            else if (Card.Location == CardLocation.Grave)
            {
                return true;
            }
            return false;
        }

        private bool DystopiaEffect()
        {
            if (Card.Location == CardLocation.MonsterZone && Card.Attack != 2800)
            {
                if (AlreadyResponded()) return false;
                return Enemy.GetMonsterCount() > 0 || Enemy.GetSpellCount() > 0;
            }
            return false;
        }

        private bool ContrastHeroChaosEffect()
        {
            if (Card.Location == CardLocation.MonsterZone)
            {
                if (AlreadyResponded()) return false;
                foreach (var c in Enemy.GetMonsters())
                {
                    if (c != null && c.IsFaceup() && !c.IsDisabled()) return true;
                }
                foreach (var c in Enemy.GetSpells())
                {
                    if (c != null && c.IsFaceup() && !c.IsDisabled()) return true;
                }
            }
            return false;
        }

        private bool MaliciousEffect()
        {
            return Card.Location == CardLocation.Grave;
        }

        private bool DenierEffect()
        {
            if (Card.Location == CardLocation.MonsterZone)
            {
                return true;
            }
            else if (Card.Location == CardLocation.Grave)
            {
                foreach (var m in Bot.GetMonsters())
                {
                    if (m != null && m.IsFaceup() && m != Card && IsDestinyHero(m))
                        return true;
                }
            }
            return false;
        }

        protected override bool EvaluateCardAction(ClientCard card, CardMetadata meta, ExecutorType type)
        {
            // Block Normal Summoning of Level 8/10 bosses
            if (type == ExecutorType.Summon)
            {
                if (card.Id == 101402021 || card.Id == 83965310 || card.Id == 40591390 || card.Id == 17132130)
                {
                    LogToTurn("Block tribute / normal summon of boss monster: " + GetCardName(card.Id));
                    return false;
                }
            }

            return base.EvaluateCardAction(card, meta, type);
        }

        // --- OVERRIDDEN DECISION HOOKS ---

        public override IList<ClientCard> OnSelectCard(IList<ClientCard> cards, int min, int max, long hint, bool cancelable)
        {
            if (hint == HintMsg_FusionMaterial)
            {
                IList<ClientCard> materials = GetOptimalFusionMaterials(cards, min, max);
                _lastSelectedFusionId = 0;
                return materials;
            }
            IList<ClientCard> selected = OnSelectCardInternal(cards, min, max, hint, cancelable);
            if (hint == HintMsg_SpSummon && selected != null && selected.Count > 0)
            {
                _lastSelectedFusionId = selected[0].Id;
            }
            return selected;
        }

        private IList<ClientCard> OnSelectCardInternal(IList<ClientCard> cards, int min, int max, long hint, bool cancelable)
        {
            if (cards == null || cards.Count == 0)
                return base.OnSelectCard(cards, min, max, hint, cancelable);

            // 1. Target Destruction (Dreadnought Servant / D-Burst / DPE)
            if (Card != null && (Card.Id == 101402023 || Card.Id == 100456010 || Card.Id == 60461804))
            {
                List<ClientCard> selected = new List<ClientCard>();
                // Prioritize Dark City (101402062) to trigger Special Summon
                foreach (var c in cards)
                {
                    if (c.Id == 101402062 && c.Location == CardLocation.SpellZone && c.Controller == 0)
                    {
                        selected.Add(c);
                        break;
                    }
                }
                // DPE self-destruction fallback
                if (selected.Count == 0 && Card.Id == 60461804)
                {
                    foreach (var c in cards)
                    {
                        if (c == Card && c.Controller == 0)
                        {
                            selected.Add(c);
                            break;
                        }
                    }
                }
                // Add remaining targets to satisfy minimum
                foreach (var c in cards)
                {
                    if (selected.Count >= max) break;
                    if (c.Controller == 0 && !selected.Contains(c))
                    {
                        selected.Add(c);
                    }
                }
                // Opponent targets for DPE
                if (Card.Id == 60461804 && selected.Count < max)
                {
                    foreach (var c in cards)
                    {
                        if (selected.Count >= max) break;
                        if (c.Controller == 1 && c.Location == CardLocation.MonsterZone && c.IsFaceup() && !selected.Contains(c))
                        {
                            selected.Add(c);
                        }
                    }
                    foreach (var c in cards)
                    {
                        if (selected.Count >= max) break;
                        if (c.Controller == 1 && !selected.Contains(c))
                        {
                            selected.Add(c);
                        }
                    }
                }
                if (selected.Count >= min) return selected;
            }

            // 2. Doom Liege Banish / Plasma Absorb Selection
            if (Card != null && (Card.Id == 101402022 || Card.Id == 83965310) && hint == 0)
            {
                ClientCard bestTarget = null;
                foreach (var c in cards)
                {
                    if (c.Controller == 1 && c.Location == CardLocation.MonsterZone && c.IsFaceup())
                    {
                        if (bestTarget == null || c.Attack > bestTarget.Attack)
                            bestTarget = c;
                    }
                }
                if (bestTarget != null)
                {
                    return new List<ClientCard> { bestTarget };
                }
            }

            // 3. Dark City / Search AddToHand Selection
            if (hint == HintMsg_AddToHand)
            {
                // Dreadnought search target selection
                if (Card != null && Card.Id == 101402037)
                {
                    List<ClientCard> selected = new List<ClientCard>();
                    foreach (var c in cards)
                    {
                        if (c.Id == 101402021 && !selected.Contains(c)) // Death Dogma
                        {
                            selected.Add(c);
                            if (selected.Count >= max) break;
                        }
                    }
                    foreach (var c in cards)
                    {
                        if (c.Id == 101402022 && !selected.Contains(c)) // Doom Liege
                        {
                            selected.Add(c);
                            if (selected.Count >= max) break;
                        }
                    }
                    foreach (var c in cards)
                    {
                        if (c.Id == 101402023 && !selected.Contains(c)) // Servant
                        {
                            selected.Add(c);
                            if (selected.Count >= max) break;
                        }
                    }
                    foreach (var c in cards)
                    {
                        if (IsDestinyHero(c) && !selected.Contains(c))
                        {
                            selected.Add(c);
                            if (selected.Count >= max) break;
                        }
                    }
                    if (selected.Count >= min) return selected;
                }

                // Dark City Field Spell search selection
                if (Card != null && Card.Id == 101402062)
                {
                    foreach (var c in cards)
                    {
                        if (c.Id == 101402022) return new List<ClientCard> { c }; // Doom Liege
                    }
                    foreach (var c in cards)
                    {
                        if (c.Id == 101402023) return new List<ClientCard> { c }; // Servant
                    }
                    foreach (var c in cards)
                    {
                        if (c.Id == 101402021) return new List<ClientCard> { c }; // Death Dogma
                    }
                }

                // Dusk Crow search selection
                if (Card != null && Card.Id == 10808715)
                {
                    foreach (var c in cards)
                    {
                        if (c.Id == 58288218) return new List<ClientCard> { c }; // Furnace
                        if (c.Id == 66206748) return new List<ClientCard> { c }; // Fountain
                    }
                }

                // Furnace search selection
                if (Card != null && Card.Id == 58288218)
                {
                    foreach (var c in cards)
                    {
                        if (c.Id == 21143940) return new List<ClientCard> { c }; // Mask Change
                        if (c.Id == 24094653) return new List<ClientCard> { c }; // Poly
                    }
                }

                // Shadow Mist search selection
                if (Card != null && Card.Id == 50720316)
                {
                    foreach (var c in cards)
                    {
                        if (c.Id == 101402022) return new List<ClientCard> { c }; // Doom Liege
                    }
                }
            }

            // 4. Fusion Material Selection (Polymerization, Death Dogma, etc.)
            if (hint == HintMsg_FusionMaterial)
            {
                return GetOptimalFusionMaterials(cards, min, max);
            }

            // 5. Denier Recycle target selection
            if (Card != null && Card.Id == 16605586)
            {
                foreach (var c in cards)
                {
                    if (c.Id == 9411399) return new List<ClientCard> { c }; // Malicious
                }
                foreach (var c in cards)
                {
                    if (IsDestinyHero(c)) return new List<ClientCard> { c };
                }
            }

            // 6. Vyon Send-to-GY selection
            if (Card != null && Card.Id == 27780618 && hint == 0)
            {
                foreach (var c in cards)
                {
                    if (c.Id == 50720316) return new List<ClientCard> { c }; // Shadow Mist
                }
                foreach (var c in cards)
                {
                    if (c.Id == 9411399) return new List<ClientCard> { c }; // Malicious
                }
            }

            // 7. Mask Change target selection
            if (Card != null && Card.Id == 21143940)
            {
                foreach (var c in cards)
                {
                    if (c.Id == 50720316 && c.Location == CardLocation.MonsterZone)
                        return new List<ClientCard> { c }; // Shadow Mist
                }
                foreach (var c in cards)
                {
                    if (c.Location == CardLocation.MonsterZone && c.Attacked && IsHero(c))
                        return new List<ClientCard> { c };
                }
                foreach (var c in cards)
                {
                    if (c.Location == CardLocation.MonsterZone && IsHero(c))
                        return new List<ClientCard> { c };
                }
            }

            // 8. Fusion Summon target selection from Extra Deck (Fusion Destiny / Polymerization)
            if (hint == HintMsg_SpSummon && max == 1)
            {
                foreach (var c in cards)
                {
                    if (c.Id == 60461804) return new List<ClientCard> { c }; // DPE
                }
                foreach (var c in cards)
                {
                    if (c.Id == 101402037) return new List<ClientCard> { c }; // Dreadnought
                }
                foreach (var c in cards)
                {
                    if (c.Id == 90579153) return new List<ClientCard> { c }; // Dystopia
                }
                foreach (var c in cards)
                {
                    if (c.Id == 30757127) return new List<ClientCard> { c }; // Dangerous
                }
            }

            return base.OnSelectCard(cards, min, max, hint, cancelable);
        }

        public override int OnSelectOption(IList<long> options)
        {
            if (options.Contains(1249831408) || options.Contains(1249831409))
            {
                if (Bot.LifePoints <= 4500)
                {
                    for (int i = 0; i < options.Count; ++i)
                    {
                        if (options[i] == 1249831408)
                        {
                            LogToTurn("Solemn Report Option: LP is low (" + Bot.LifePoints + "), selecting 1500 LP option.");
                            return i;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < options.Count; ++i)
                    {
                        if (options[i] == 1249831409)
                        {
                            LogToTurn("Solemn Report Option: LP is high (" + Bot.LifePoints + "), selecting 3000 LP option.");
                            return i;
                        }
                    }
                }
            }
            return base.OnSelectOption(options);
        }

        private IList<ClientCard> GetOptimalFusionMaterials(IList<ClientCard> cards, int min, int max)
        {
            List<List<ClientCard>> combos = GetCombinations(cards, min);
            List<List<ClientCard>> validCombos = new List<List<ClientCard>>();

            foreach (var combo in combos)
            {
                bool isValid = false;
                if (_lastSelectedFusionId == 60461804) // DPE
                {
                    isValid = IsDpeRecipe(combo);
                }
                else if (_lastSelectedFusionId == 101402037) // Dreadnought
                {
                    isValid = IsDreadnoughtRecipe(combo);
                }
                else if (_lastSelectedFusionId == 90579153) // Dystopia
                {
                    isValid = IsDystopiaRecipe(combo);
                }
                else if (_lastSelectedFusionId == 30757127) // Dangerous
                {
                    isValid = IsDangerousRecipe(combo);
                }
                else if (_lastSelectedFusionId == 46759931) // Trinity
                {
                    isValid = IsTrinityRecipe(combo);
                }
                else if (_lastSelectedFusionId == 23204029) // Contrast HERO Chaos
                {
                    isValid = IsContrastHeroChaosRecipe(combo);
                }
                else
                {
                    isValid = IsDpeRecipe(combo) || IsDreadnoughtRecipe(combo) || IsDystopiaRecipe(combo) || IsDangerousRecipe(combo) || IsTrinityRecipe(combo) || IsContrastHeroChaosRecipe(combo);
                }

                if (isValid)
                {
                    validCombos.Add(combo);
                }
            }

            if (validCombos.Count > 0)
            {
                List<ClientCard> bestCombo = null;
                double bestScore = double.MinValue;
                foreach (var combo in validCombos)
                {
                    double score = ScoreCombination(combo);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestCombo = combo;
                    }
                }
                if (bestCombo != null)
                {
                    return bestCombo;
                }
            }

            List<ClientCard> sorted = new List<ClientCard>(cards);
            sorted.Sort((a, b) =>
            {
                double scoreA = ScoreCardIndividual(a);
                double scoreB = ScoreCardIndividual(b);
                return scoreB.CompareTo(scoreA);
            });

            List<ClientCard> fallbackResult = new List<ClientCard>();
            for (int i = 0; i < Math.Min(min, sorted.Count); i++)
            {
                fallbackResult.Add(sorted[i]);
            }
            return fallbackResult;
        }

        private double ScoreCombination(List<ClientCard> combo)
        {
            double totalScore = 0.0;
            foreach (var card in combo)
            {
                if (card == null) continue;
                double cardScore = ScoreCardIndividual(card);
                totalScore += cardScore;
            }
            return totalScore;
        }

        private double ScoreCardIndividual(ClientCard card)
        {
            if (card == null) return 0.0;
            double cardScore = 0.0;
            CardMetadata meta = GetOrCreateMetadata(card);
            if (meta != null)
            {
                cardScore -= meta.priority * 2.0;
            }
            if (card.Location == CardLocation.Grave)
            {
                cardScore += 15.0;
            }
            else if (card.Location == CardLocation.Hand)
            {
                cardScore += 5.0;
            }
            if (card.Id == 9411399 || card.Id == 16605586 || card.Id == 101402023)
            {
                cardScore += 25.0;
            }
            if (card.Id == 86120751 || card.Id == 101305015)
            {
                if (card.Location == CardLocation.Grave || card.Location == CardLocation.MonsterZone)
                {
                    cardScore += 20.0;
                }
            }
            if (card.Location == CardLocation.Hand && 
                (card.Id == 14558127 || card.Id == 10045474 || card.Id == 24224830 || card.Id == 73642296 || card.Id == 42141493 || card.Id == 94145021 || card.Id == 6325660 || card.Id == 78114463))
            {
                cardScore -= 50.0;
            }
            if (card.Id == 101305017)
            {
                cardScore -= 30.0;
            }
            return cardScore;
        }

        private List<List<ClientCard>> GetCombinations(IList<ClientCard> list, int k)
        {
            List<List<ClientCard>> result = new List<List<ClientCard>>();
            GetCombinationsRec(list, k, 0, new List<ClientCard>(), result);
            return result;
        }

        private void GetCombinationsRec(IList<ClientCard> list, int k, int start, List<ClientCard> current, List<List<ClientCard>> result)
        {
            if (current.Count == k)
            {
                result.Add(new List<ClientCard>(current));
                return;
            }
            for (int i = start; i < list.Count; i++)
            {
                current.Add(list[i]);
                GetCombinationsRec(list, k, i + 1, current, result);
                current.RemoveAt(current.Count - 1);
            }
        }

        private bool IsMaskedHero(ClientCard card)
        {
            if (card == null) return false;
            int id = card.Id;
            return id == 58288218 || id == 66206748 || id == 10808715 || id == 58481572 || id == 54757758 || id == 69394324 || id == 63813056 || id == 93657021;
        }

        private bool IsDpeRecipe(List<ClientCard> combo)
        {
            if (combo.Count != 2) return false;
            ClientCard c1 = combo[0];
            ClientCard c2 = combo[1];
            bool case1 = (IsHero(c1) && c1.Level >= 6) && IsDestinyHero(c2);
            bool case2 = (IsHero(c2) && c2.Level >= 6) && IsDestinyHero(c1);
            return case1 || case2;
        }

        private bool IsDreadnoughtRecipe(List<ClientCard> combo)
        {
            if (combo.Count != 2) return false;
            return IsDestinyHero(combo[0]) && combo[0].Level >= 5 && IsDestinyHero(combo[1]) && combo[1].Level >= 5;
        }

        private bool IsDystopiaRecipe(List<ClientCard> combo)
        {
            if (combo.Count != 2) return false;
            return IsDestinyHero(combo[0]) && IsDestinyHero(combo[1]);
        }

        private bool IsDangerousRecipe(List<ClientCard> combo)
        {
            if (combo.Count != 2) return false;
            ClientCard c1 = combo[0];
            ClientCard c2 = combo[1];
            bool case1 = IsDestinyHero(c1) && (c2.Attribute == (int)CardAttribute.Dark && c2.HasType(CardType.Effect));
            bool case2 = IsDestinyHero(c2) && (c1.Attribute == (int)CardAttribute.Dark && c1.HasType(CardType.Effect));
            return case1 || case2;
        }

        private bool IsTrinityRecipe(List<ClientCard> combo)
        {
            if (combo.Count != 3) return false;
            return IsHero(combo[0]) && IsHero(combo[1]) && IsHero(combo[2]);
        }

        private bool IsContrastHeroChaosRecipe(List<ClientCard> combo)
        {
            if (combo.Count != 2) return false;
            return IsMaskedHero(combo[0]) && IsMaskedHero(combo[1]);
        }
    }
}
