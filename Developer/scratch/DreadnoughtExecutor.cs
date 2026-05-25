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
        private const int HintMsg_FusionMaterial = 511;

        public DreadnoughtExecutor(GameAI ai, Duel duel) : base(ai, duel)
        {
            // Clear default weight-based dynamic registry to enforce strict, prioritized sequencing rules (BaseRules)
            Executors.Clear();

            // 1. Hand traps, interruptions, and negates (high priority response handlers)
            AddExecutor(ExecutorType.Activate, 14558127, AshBlossomEffect); // Ash Blossom
            AddExecutor(ExecutorType.Activate, 10045474, ImpermanenceEffect); // Infinite Impermanence
            AddExecutor(ExecutorType.Activate, 24224830, CalledByGraveEffect); // Called by the Grave (ID 1)
            AddExecutor(ExecutorType.Activate, 24224830, CalledByGraveEffect); // Called by the Grave (ID 2)
            AddExecutor(ExecutorType.Activate, 42141493, FuwalosEffect); // Mulcharmy Fuwalos
            AddExecutor(ExecutorType.Activate, 94145021, DrollEffect); // Droll & Lock Bird
            AddExecutor(ExecutorType.Activate, 6325660, DominusSparkEffect); // Dominus Spark

            // 2. Search & Field Spells
            AddExecutor(ExecutorType.Activate, 73628505, TerraformingEffect); // Terraforming
            AddExecutor(ExecutorType.Activate, 101402062, DarkCityFieldEffect); // Clock Tower Prison City - Dark City Field Activation
            AddExecutor(ExecutorType.Activate, 101402022, DoomLiegeEffect); // Doom Liege (Banish/Search)
            AddExecutor(ExecutorType.Activate, 40237839, SabatielEffect); // Sabatiel (Search Poly/Fusion)

            // 3. Extenders & Special Summons (Hand/GY)
            AddExecutor(ExecutorType.SpSummon, 101402023, DreadnoughtServantSpSummon); // Dreadnought Servant hand SS
            AddExecutor(ExecutorType.Activate, 101402023, DreadnoughtServantEffect); // Dreadnought Servant (Destroy/Search or GY spin)
            AddExecutor(ExecutorType.SpSummon, 10808715, DuskCrowSpSummon); // Dusk Crow SS
            AddExecutor(ExecutorType.Activate, 10808715, DuskCrowEffect); // Dusk Crow search
            AddExecutor(ExecutorType.Activate, 58288218, MaskedHeroFurnaceEffect); // Furnace search
            AddExecutor(ExecutorType.SpSummon, 58288218, MaskedHeroFurnaceSpSummon); // Furnace SS from GY
            AddExecutor(ExecutorType.Activate, 66206748, MaskedHeroFountainEffect); // Fountain SS/Set Mask Change
            AddExecutor(ExecutorType.SpSummon, 101402021, DeathDogmaSpSummon); // Death Dogma SS
            AddExecutor(ExecutorType.Activate, 101402021, DeathDogmaEffect); // Death Dogma (Burn or Quick Fusion)
            AddExecutor(ExecutorType.SpSummon, 83965310, PlasmaSpSummon); // Plasma SS
            AddExecutor(ExecutorType.Activate, 83965310, PlasmaEffect); // Plasma Absorb

            // 4. Normal Summons
            AddExecutor(ExecutorType.Summon, 27780618, VyonSummon); // Vision HERO Vyon
            AddExecutor(ExecutorType.Summon, 101402022, DoomLiegeSummon); // Doom Liege
            AddExecutor(ExecutorType.Summon, 50720316, ShadowMistSummon); // Shadow Mist
            AddExecutor(ExecutorType.Summon, 101402023, DreadnoughtServantSummon); // Servant

            // 5. Summon Trigger Effects & Searches
            AddExecutor(ExecutorType.Activate, 27780618, VyonEffect); // Vyon Send/Search Poly
            AddExecutor(ExecutorType.Activate, 50720316, ShadowMistEffect); // Shadow Mist Search

            // 6. Fusion & Spell Engines
            AddExecutor(ExecutorType.Activate, 52947044, FusionDestinyEffect); // Fusion Destiny
            AddExecutor(ExecutorType.Activate, 24094653, PolymerizationEffect); // Polymerization
            AddExecutor(ExecutorType.Activate, 21143940, MaskChangeEffect); // Mask Change
            AddExecutor(ExecutorType.Activate, 48130397, SuperPolymerizationEffect); // Super Poly
            AddExecutor(ExecutorType.Activate, 100456010, DBurstEffect); // D-Burst (Destroy/SS or GY Double Attack)

            // 7. Extra Deck Proc & Boss Triggers
            AddExecutor(ExecutorType.SpSummon, 101402037, DreadnoughtSpSummon); // Dreadnought Alt Summon Proc
            AddExecutor(ExecutorType.Activate, 101402037, DreadnoughtEffect); // Dreadnought Search
            AddExecutor(ExecutorType.Activate, 60461804, DPEEffect); // DPE Destroy/Rebirth
            AddExecutor(ExecutorType.Activate, 90579153, DystopiaEffect); // Dystopia Destroy
            AddExecutor(ExecutorType.Activate, 23204029, ContrastHeroChaosEffect); // Contrast HERO Chaos Negate
            AddExecutor(ExecutorType.Activate, 9411399, MaliciousEffect); // Malicious Summon another copy
            AddExecutor(ExecutorType.Activate, 16605586, DenierEffect); // Denier Recycle/SS

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

        private bool AshBlossomEffect()
        {
            // Disrupt opponent on their turn responding to their cards (Iron Rule #1 & #6)
            return Duel.Player == 1 && Util.GetLastChainCard() != null && Util.GetLastChainCard().Controller == 1;
        }

        private bool ImpermanenceEffect()
        {
            // Iron Rule #1 & #3
            if (Duel.Player == 0) return false;
            foreach (var m in Enemy.GetMonsters())
            {
                if (m != null && m.IsFaceup() && !m.IsDisabled())
                    return true;
            }
            return false;
        }

        private bool CalledByGraveEffect()
        {
            // Iron Rule #3: require target in opponent's GY
            ClientCard lastChainCard = Util.GetLastChainCard();
            if (lastChainCard != null && lastChainCard.Controller == 1 && lastChainCard.IsMonster())
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
            // Iron Rule #1: only activate on opponent's turn
            return Duel.Player == 1;
        }

        private bool DrollEffect()
        {
            // Iron Rule #1: only activate on opponent's turn
            return Duel.Player == 1;
        }

        private bool DominusSparkEffect()
        {
            // Iron Rule #1: only activate on opponent's turn
            return Duel.Player == 1;
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
                return Enemy.GetMonsterCount() > 0 || Enemy.GetSpellCount() > 0;
            }
            return false;
        }

        private bool ContrastHeroChaosEffect()
        {
            if (Card.Location == CardLocation.MonsterZone)
            {
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
                List<ClientCard> selected = new List<ClientCard>();
                // Prioritize recycling Malicious, Denier, or Servant
                foreach (var c in cards)
                {
                    if ((c.Id == 9411399 || c.Id == 16605586 || c.Id == 101402023) && !selected.Contains(c))
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
                foreach (var c in cards)
                {
                    if (!selected.Contains(c))
                    {
                        selected.Add(c);
                        if (selected.Count >= max) break;
                    }
                }
                if (selected.Count >= min) return selected;
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

            return base.OnSelectCard(cards, min, max, hint, cancelable);
        }
    }
}
