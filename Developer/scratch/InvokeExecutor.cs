using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using WindBot.Game;
using WindBot.Game.AI;
using YGOSharp.OCGWrapper.Enums;

namespace ProjectIgnisAI
{
    [Deck("2026_Invoke", "2026_Invoke")]
    public class InvokeExecutor : UnifiedIgnisExecutor
    {
        // Selection hints constants
        private const int HintMsg_Discard = 501;
        private const int HintMsg_AddToHand = 506;
        private const int HintMsg_FusionMaterial = 511;
        private const int HintMsg_LinkMaterial = 533;

        public InvokeExecutor(GameAI ai, Duel duel) : base(ai, duel)
        {
            // Clear default weight-based dynamic registry to enforce strict, prioritized sequencing rules (BaseRules)
            Executors.Clear();

            // 1. Hand traps, interruptions, and negates (high priority response handlers)
            AddExecutor(ExecutorType.Activate, 14558127, AshBlossomEffect); // Ash Blossom
            AddExecutor(ExecutorType.Activate, 73642296, GhostBelleEffect); // Ghost Belle
            AddExecutor(ExecutorType.Activate, 10045474, ImpermanenceEffect); // Infinite Impermanence
            AddExecutor(ExecutorType.Activate, 24224830, CalledByGraveEffect); // Called by the Grave

            // 2. Search & Field Spells
            AddExecutor(ExecutorType.Activate, 73628505, TerraformingEffect); // Terraforming
            AddExecutor(ExecutorType.Activate, 47679935, MagicalMeltdownEffect); // Magical Meltdown
            AddExecutor(ExecutorType.Activate, 89739383, SpellbookOfSecretsEffect); // Spellbook of Secrets

            // 3. Extenders & Special Summons
            AddExecutor(ExecutorType.SpSummon, 101305017, VirakamSpSummonEffect); // Virakam the Artificial Spirit
            AddExecutor(ExecutorType.Activate, 101305017, VirakamEffect); // Virakam Effect
            AddExecutor(ExecutorType.Activate, 101305015, AleisterReminiscentHandEffect); // Aleister the Reminiscent hand SS

            // 4. Normal Summons
            AddExecutor(ExecutorType.Summon, 86120751, AleisterSummonEffect); // Aleister the Invoker
            AddExecutor(ExecutorType.Summon, 14824019, SpellbookMagicianSummonEffect); // Spellbook Magician of Prophecy

            // 5. Summon Trigger Effects & Searches
            AddExecutor(ExecutorType.Activate, 86120751, AleisterFieldEffect);
            AddExecutor(ExecutorType.Activate, 14824019, SpellbookMagicianFieldEffect);
            AddExecutor(ExecutorType.Activate, 101305015, AleisterReminiscentSummonedEffect);

            // 6. Draw Engine (run after summoning/searching Spellbook components)
            AddExecutor(ExecutorType.Activate, 23314220, SpellbookOfKnowledgeEffect);

            // 7. Custom Archetype Setup Spells
            AddExecutor(ExecutorType.Activate, 101305054, SpiritSwordAiwassEffect);
            AddExecutor(ExecutorType.Activate, 101305070, RosaMundiEffect);

            // 8. Link Summons
            AddExecutor(ExecutorType.SpSummon, 34755994, ArtemisSummonEffect); // Artemis, Magistus Link-1

            // 9. Fusion Summons
            AddExecutor(ExecutorType.Activate, 74063034, InvocationEffect); // Invocation
            AddExecutor(ExecutorType.Activate, 101305053, InvocationSwordEffect); // Invocation "Sword"

            // 10. Boss Monster Effects
            AddExecutor(ExecutorType.Activate, 101305016, AiwassSpiritOfTheLawEffect); // Aiwass the Spirit of the Law
            AddExecutor(ExecutorType.Activate, 75286621, MechabaEffect); // Invoked Mechaba
            AddExecutor(ExecutorType.Activate, 38423248, AugoeidesEffect); // Invoked Augoeides
            AddExecutor(ExecutorType.Activate, 101305033, TranscendenceAeonEffect); // Invoked Transcendence Aeon
            AddExecutor(ExecutorType.Activate, 101305031, BabalonEffect); // Invoked Babalon
            AddExecutor(ExecutorType.Activate, 101305030, SorathEffect); // Invoked Sorath
            AddExecutor(ExecutorType.Activate, 101305032, OkeanosEffect); // Invoked Okeanos
            AddExecutor(ExecutorType.Activate, 12307878, ElysiumEffect); // Invoked Elysium
            AddExecutor(ExecutorType.Activate, 49513164, RaidjinEffect); // Invoked Raidjin
            AddExecutor(ExecutorType.Activate, 13529466, PurgatrioEffect); // Invoked Purgatrio
            AddExecutor(ExecutorType.Activate, 97973962, CaligaEffect); // Invoked Caliga
            AddExecutor(ExecutorType.Activate, 23656668, MagellanicaEffect); // Invoked Magellanica

            // 11. Hand Combat Boost (guarded to Battle Phase only to save resources)
            AddExecutor(ExecutorType.Activate, 86120751, AleisterHandEffect);

            // 12. Catch-All Fallbacks
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

        private bool HasAleister()
        {
            if (Bot == null) return false;
            foreach (var card in Bot.GetMonsters())
            {
                if (card != null && card.IsFaceup() && (card.Id == 86120751 || card.Id == 101305015))
                    return true;
            }
            foreach (var card in Bot.Graveyard)
            {
                if (card != null && (card.Id == 86120751 || card.Id == 101305015))
                    return true;
            }
            return false;
        }

        // --- SPECIFIC CARD RULE METHODS ---

        private bool AshBlossomEffect()
        {
            // Disrupt opponent on their turn responding to their cards
            return Duel.Player == 1 && Util.GetLastChainCard() != null && Util.GetLastChainCard().Controller == 1;
        }

        private bool GhostBelleEffect()
        {
            return Duel.Player == 1 && Util.GetLastChainCard() != null && Util.GetLastChainCard().Controller == 1;
        }

        private bool ImpermanenceEffect()
        {
            // Negate face-up active opponent monster
            foreach (var m in Enemy.GetMonsters())
            {
                if (m != null && m.IsFaceup() && !m.IsDisabled())
                    return true;
            }
            return false;
        }

        private bool CalledByGraveEffect()
        {
            // Only activate to respond to opponent's monster activations (especially handtraps or GY effects)
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

        private bool TerraformingEffect()
        {
            // Search Meltdown if not already active or held in hand
            return !HasInSpellZone(47679935) && !HasInHand(47679935);
        }

        private bool MagicalMeltdownEffect()
        {
            if (Card.Location != CardLocation.Hand) return false;
            return !HasInSpellZone(47679935);
        }

        private bool SpellbookOfSecretsEffect()
        {
            return true;
        }

        private bool VirakamSpSummonEffect()
        {
            return HasAleister();
        }

        private bool VirakamEffect()
        {
            return true;
        }

        private bool AleisterReminiscentHandEffect()
        {
            if (Card.Location != CardLocation.Hand) return false;
            // Target a face-up Spellcaster we control to boost and summon
            foreach (var m in Bot.GetMonsters())
            {
                if (m != null && m.IsFaceup() && (m.Race == (int)CardRace.SpellCaster || m.HasType(CardType.Fusion)))
                    return true;
            }
            return false;
        }

        private bool AleisterSummonEffect()
        {
            // Prioritize Field Setup Spells first if we hold them
            if (!HasInSpellZone(47679935))
            {
                if (HasInHand(47679935) || HasInHand(73628505))
                    return false;
            }
            return true;
        }

        private bool SpellbookMagicianSummonEffect()
        {
            // Summon Magician if we don't have Aleister, or to run draw engine combo
            if (HasInHand(86120751))
            {
                return HasInHand(23314220) || HasInHand(89739383);
            }
            return true;
        }

        private bool AleisterFieldEffect()
        {
            return Card.Location == CardLocation.MonsterZone;
        }

        private bool SpellbookMagicianFieldEffect()
        {
            return Card.Location == CardLocation.MonsterZone;
        }

        private bool AleisterReminiscentSummonedEffect()
        {
            return Card.Location == CardLocation.MonsterZone;
        }

        private bool SpellbookOfKnowledgeEffect()
        {
            // Draw 2 by tributing/sending a Spellcaster (do not send fusion bosses)
            foreach (var m in Bot.GetMonsters())
            {
                if (m != null && m.IsFaceup() && (m.Id == 14824019 || m.Id == 34755994 || m.Id == 101305015))
                    return true;
            }
            return false;
        }

        private bool SpiritSwordAiwassEffect()
        {
            return true;
        }

        private bool RosaMundiEffect()
        {
            // Activate in opponent's turn to drop Mechaba/Okeanos as disruption
            if (Duel.Player == 1)
            {
                if (Duel.CurrentChain.Count > 0 || Enemy.GetMonsterCount() > 0 || Enemy.GetSpellCount() > 0)
                {
                    return true;
                }
            }
            // Activate on our turn during Main Phase to extend
            if (Duel.Player == 0 && Duel.Phase == DuelPhase.Main1)
            {
                return true;
            }
            return false;
        }

        private bool ArtemisSummonEffect()
        {
            // Link away Aleister into Artemis (LIGHT material) if we have Invocation available
            bool hasAleister = false;
            foreach (var m in Bot.GetMonsters())
            {
                if (m != null && m.IsFaceup() && m.Id == 86120751)
                    hasAleister = true;
            }
            if (!hasAleister) return false;

            return HasInHand(74063034) || HasInHand(101305053) || HasInHand(86120751) || HasInHand(101305015) || HasInGraveyard(74063034) || HasInGraveyard(101305053);
        }

        private bool InvocationEffect()
        {
            return true;
        }

        private bool InvocationSwordEffect()
        {
            return true;
        }

        private bool AiwassSpiritOfTheLawEffect()
        {
            if (Card.Location == CardLocation.Hand)
            {
                // Banish from hand only if we need to search Aleister starter
                return !HasInHand(86120751) && !HasInHand(101305015);
            }
            return true;
        }

        private bool MechabaEffect()
        {
            return true;
        }

        private bool AugoeidesEffect()
        {
            return true;
        }

        private bool TranscendenceAeonEffect()
        {
            // If it's opponent's turn, declare DARK to disrupt LIGHT machine plays (like ABC / Cyber Dragon)
            if (Duel.Player == 1)
            {
                if (Enemy.GetMonsterCount() > 0)
                {
                    AI.SelectAttribute(CardAttribute.Dark);
                    return true;
                }
            }
            // On our turn, declare Light to enable Light-based synergies if needed
            if (Duel.Player == 0)
            {
                AI.SelectAttribute(CardAttribute.Light);
                return true;
            }
            return true;
        }

        private bool BabalonEffect()
        {
            return true;
        }

        private bool SorathEffect()
        {
            return true;
        }

        private bool OkeanosEffect()
        {
            return true;
        }

        private bool ElysiumEffect()
        {
            return true;
        }

        private bool RaidjinEffect()
        {
            return true;
        }

        private bool PurgatrioEffect()
        {
            return true;
        }

        private bool CaligaEffect()
        {
            return true;
        }

        private bool MagellanicaEffect()
        {
            return true;
        }

        private bool AleisterHandEffect()
        {
            // Block hand ATK boost outside the Battle Phase to save resources
            if (Card.Location != CardLocation.Hand || Duel.Phase != DuelPhase.Battle) return false;

            foreach (var m in Bot.GetMonsters())
            {
                if (m != null && m.IsFaceup() && m.HasType(CardType.Fusion))
                    return true;
            }
            return false;
        }

        protected override bool EvaluateCardAction(ClientCard card, CardMetadata meta, ExecutorType type)
        {
            int cardId = card.Id;

            // Block Normal Summoning of Level 6 Aiwass (it requires tributes and should only be Special Summoned)
            if (cardId == 101305016 && type == ExecutorType.Summon)
            {
                LogToTurn("Block tribute / normal summon of Aiwass the Spirit of the Law.");
                return false;
            }

            return base.EvaluateCardAction(card, meta, type);
        }

        // --- OVERRIDDEN DECISION HOOKS ---

        public override IList<ClientCard> OnSelectCard(IList<ClientCard> cards, int min, int max, long hint, bool cancelable)
        {
            if (cards == null || cards.Count == 0)
                return base.OnSelectCard(cards, min, max, hint, cancelable);

            // Sniping Extra Deck threats via Spirit Sword Aiwass (Option 2)
            if (Card != null && Card.Id == 101305054)
            {
                foreach (var c in cards)
                {
                    if (c.Id == 1561110) return new List<ClientCard> { c }; // ABC-Dragon Buster
                }
                foreach (var c in cards)
                {
                    if (c.Id == 10443957) return new List<ClientCard> { c }; // Cyber Dragon Infinity
                }
                foreach (var c in cards)
                {
                    if (c.Id == 4280258) return new List<ClientCard> { c }; // Apollousa
                }
                foreach (var c in cards)
                {
                    if (c.Id == 29301450) return new List<ClientCard> { c }; // S:P Little Knight
                }
            }

            // Summoning target selection for Magical Name - "Rosa Mundi"
            if (Card != null && Card.Id == 101305070)
            {
                if (Bot.Hand.Count > 0)
                {
                    foreach (var c in cards)
                    {
                        if (c.Id == 75286621) return new List<ClientCard> { c }; // Mechaba
                    }
                }
                foreach (var c in cards)
                {
                    if (c.Id == 101305032) return new List<ClientCard> { c }; // Okeanos
                }
                foreach (var c in cards)
                {
                    if (c.Id == 101305031) return new List<ClientCard> { c }; // Babalon
                }
            }

            // 1. Link Material Selection (Artemis, Magistus Moon Maiden)
            if (hint == HintMsg_LinkMaterial)
            {
                foreach (var c in cards)
                {
                    if (c.Id == 86120751 && c.Location == CardLocation.MonsterZone)
                    {
                        LogToTurn("OnSelectCard (Link Material): Selecting Aleister the Invoker for Artemis.");
                        return new List<ClientCard> { c };
                    }
                }
            }

            // 2. Fusion Material Selection (Invocation / Invocation Sword)
            if (hint == HintMsg_FusionMaterial)
            {
                List<ClientCard> selected = new List<ClientCard>();
                
                // Prioritize Aleisters from field/GY
                foreach (var c in cards)
                {
                    if (c.Id == 86120751 || c.Id == 101305015)
                    {
                        selected.Add(c);
                        if (selected.Count >= max) break;
                    }
                }
                
                // Prioritize Artemis link material
                if (selected.Count < max)
                {
                    foreach (var c in cards)
                    {
                        if (c.Id == 34755994 && !selected.Contains(c))
                        {
                            selected.Add(c);
                            if (selected.Count >= max) break;
                        }
                    }
                }
                
                // Prioritize other LIGHT monsters (Babalon, Veiler)
                if (selected.Count < max)
                {
                    foreach (var c in cards)
                    {
                        if ((c.Id == 101305031 || c.Id == 10045474) && !selected.Contains(c))
                        {
                            selected.Add(c);
                            if (selected.Count >= max) break;
                        }
                    }
                }
                
                // fallback materials, avoid Virakam (101305017) if possible to keep its negate active
                if (selected.Count < max)
                {
                    foreach (var c in cards)
                    {
                        if (c.Id != 101305017 && !selected.Contains(c))
                        {
                            selected.Add(c);
                            if (selected.Count >= max) break;
                        }
                    }
                }
                
                // If still not enough, use Virakam
                if (selected.Count < max)
                {
                    foreach (var c in cards)
                    {
                        if (c.Id == 101305017 && !selected.Contains(c))
                        {
                            selected.Add(c);
                            if (selected.Count >= max) break;
                        }
                    }
                }
                
                if (selected.Count >= min)
                {
                    LogToTurn("OnSelectCard (Fusion Material): Selected optimized materials.");
                    return selected;
                }
            }

            // 3. Spellbook of Knowledge target (send Spellcaster to GY for draw)
            if (Card != null && Card.Id == 23314220)
            {
                List<ClientCard> selected = new List<ClientCard>();
                
                // Prioritize Spellbook Magician of Prophecy
                foreach (var c in cards)
                {
                    if (c.Id == 14824019 && c.Location == CardLocation.MonsterZone)
                    {
                        selected.Add(c);
                        break;
                    }
                }
                
                // Artemis Link-1
                if (selected.Count == 0)
                {
                    foreach (var c in cards)
                    {
                        if (c.Id == 34755994 && c.Location == CardLocation.MonsterZone)
                        {
                            selected.Add(c);
                            break;
                        }
                    }
                }
                
                // Aleister (if no other choice)
                if (selected.Count == 0)
                {
                    foreach (var c in cards)
                    {
                        if (c.Id == 86120751 && c.Location == CardLocation.MonsterZone)
                        {
                            selected.Add(c);
                            break;
                        }
                    }
                }
                
                if (selected.Count >= min)
                {
                    LogToTurn("OnSelectCard (Spellbook of Knowledge): Selected target to send to GY.");
                    return selected;
                }
            }

            // 4. Add to Hand / Search Selection
            if (hint == HintMsg_AddToHand)
            {
                if (Card != null)
                {
                    // Aleister the Invoker Search
                    if (Card.Id == 86120751)
                    {
                        foreach (var c in cards)
                        {
                            if (c.Id == 74063034) return new List<ClientCard> { c }; // Invocation
                        }
                        foreach (var c in cards)
                        {
                            if (c.Id == 101305053) return new List<ClientCard> { c }; // Invocation "Sword"
                        }
                        foreach (var c in cards)
                        {
                            if (c.Id == 101305054) return new List<ClientCard> { c }; // Spirit Sword Aiwass
                        }
                    }

                    // Aleister the Reminiscent Search
                    if (Card.Id == 101305015)
                    {
                        foreach (var c in cards)
                        {
                            if (c.Id == 74063034) return new List<ClientCard> { c }; // regular Invocation (crucial!)
                        }
                        foreach (var c in cards)
                        {
                            if (c.Id == 101305053) return new List<ClientCard> { c }; // Invocation "Sword"
                        }
                        foreach (var c in cards)
                        {
                            if (c.Id == 101305054) return new List<ClientCard> { c }; // Spirit Sword Aiwass
                        }
                        foreach (var c in cards)
                        {
                            if (c.Id == 101305070) return new List<ClientCard> { c }; // Magical Name - Rosa Mundi
                        }
                    }

                    // Aiwass the Spirit of the Law Search
                    if (Card.Id == 101305016)
                    {
                        foreach (var c in cards)
                        {
                            if (c.Id == 86120751) return new List<ClientCard> { c }; // Aleister the Invoker
                        }
                        foreach (var c in cards)
                        {
                            if (c.Id == 101305015) return new List<ClientCard> { c }; // Aleister the Reminiscent
                        }
                    }

                    // Spellbook Magician of Prophecy OR Spellbook of Secrets Search
                    if (Card.Id == 14824019 || Card.Id == 89739383)
                    {
                        foreach (var c in cards)
                        {
                            if (c.Id == 23314220) return new List<ClientCard> { c }; // Spellbook of Knowledge
                        }
                        foreach (var c in cards)
                        {
                            if (c.Id == 89739383) return new List<ClientCard> { c }; // Spellbook of Secrets
                        }
                        foreach (var c in cards)
                        {
                            if (c.Id == 14824019) return new List<ClientCard> { c }; // Spellbook Magician
                        }
                    }
                }
            }

            // 5. Discard Cost Selection
            if (hint == HintMsg_Discard)
            {
                // Discard Aiwass (has GY special summon/fusion effect)
                foreach (var c in cards)
                {
                    if (c.Id == 101305016)
                    {
                        LogToTurn("OnSelectCard (Discard): Prioritizing Aiwass the Spirit of the Law.");
                        return new List<ClientCard> { c };
                    }
                }
                
                // Discard duplicate spells
                foreach (var c in cards)
                {
                    if (c.Id == 74063034 || c.Id == 101305053 || c.Id == 47679935)
                    {
                        LogToTurn("OnSelectCard (Discard): Prioritizing duplicate/extra spell.");
                        return new List<ClientCard> { c };
                    }
                }
            }

            return base.OnSelectCard(cards, min, max, hint, cancelable);
        }

        public override int OnSelectPlace(long cardId, int player, CardLocation location, int available)
        {
            // Invoked Okeanos always place on Extra Monster Zone to activate Macro Cosmos-like effect
            if (cardId == 101305032 && location == CardLocation.MonsterZone)
            {
                if ((Zones.z5 & available) > 0) return Zones.z5;
                if ((Zones.z6 & available) > 0) return Zones.z6;
            }
            return base.OnSelectPlace(cardId, player, location, available);
        }

        public override int OnSelectOption(IList<long> options)
        {
            if (Card != null && Card.Id == 101305054) // Spirit Sword Aiwass
            {
                // If we don't have Aleister or Meltdown in hand, we need a starter -> Select Option 0 (SS Aiwass)
                if (!HasInHand(86120751) && !HasInHand(101305015) && !HasInHand(47679935))
                {
                    LogToTurn("OnSelectOption (Spirit Sword Aiwass): Selecting Option 0 (SS Aiwass from Deck).");
                    return 0;
                }
                // If we have starter and opponent has cards in Extra Deck, let's snipe their Extra Deck (Option 2)
                if (options.Count > 2)
                {
                    LogToTurn("OnSelectOption (Spirit Sword Aiwass): Sniping opponent's Extra Deck (Option 2).");
                    return 2;
                }
                return 0;
            }
            return base.OnSelectOption(options);
        }

        public override bool OnSelectYesNo(long desc)
        {
            return true;
        }
    }
}
