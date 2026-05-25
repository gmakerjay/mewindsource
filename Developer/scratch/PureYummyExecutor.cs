using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using WindBot.Game;
using WindBot.Game.AI;
using YGOSharp.OCGWrapper.Enums;

namespace ProjectIgnisAI
{
    [Deck("2026_PureYummy", "2026_PureYummy")]
    public class PureYummyExecutor : UnifiedIgnisExecutor
    {
        public PureYummyExecutor(GameAI ai, Duel duel) : base(ai, duel)
        {
        }

        protected override bool EvaluateCardAction(ClientCard card, CardMetadata meta, ExecutorType type)
        {
            // --- STAPLE SPELLS/TRAPS & EXTENDERS ---

            // 1. One for One (2295440)
            if (type == ExecutorType.Activate && card.Id == 2295440)
            {
                LogToTurn("Activating One for One to special summon Level 1 Yummy.");
                return true;
            }

            // 2. Piri Reis Map (33907039)
            if (type == ExecutorType.Activate && card.Id == 33907039)
            {
                if (Duel.Player == 0 && Duel.Phase == DuelPhase.Main1 && Duel.Turn == 1)
                {
                    LogToTurn("Activating Piri Reis Map to search Cupsy☆Yummy.");
                    return true;
                }
            }

            // 3. Yummy★Snatchy (30581601) - Link-1
            if (type == ExecutorType.SpSummon && card.Id == 30581601)
            {
                // Link summon Snatchy if we have any Level 1 Yummy monster on the field
                bool hasMaterial = false;
                foreach (var m in Bot.GetMonsters())
                {
                    if (m != null && m.IsFaceup() && (m.Id == 31425736 || m.Id == 10966439 || m.Id == 4215180 || m.Id == 68810435))
                    {
                        hasMaterial = true;
                        break;
                    }
                }
                if (hasMaterial)
                {
                    LogToTurn("Link Summoning Yummy★Snatchy using a Level 1 monster.");
                    return true;
                }
            }

            // Yummy★Snatchy Quick-Synchro on opponent's turn
            if (type == ExecutorType.Activate && card.Id == 30581601 && Duel.Player == 1)
            {
                LogToTurn("Yummy★Snatchy Quick-Synchro triggered on opponent's turn.");
                return true;
            }

            // 4. Yummyusment☆Mignon (66975205) - Field Spell
            if (card.Id == 66975205)
            {
                if (type == ExecutorType.Activate)
                {
                    // Card activation from hand
                    if (card.Location == CardLocation.Hand)
                    {
                        LogToTurn("Activating Yummyusment☆Mignon from hand.");
                        return true;
                    }
                    // On-field effect to revive Level 1 Yummy
                    bool hasSnatchy = false;
                    foreach (var m in Bot.GetMonsters())
                    {
                        if (m != null && m.IsFaceup() && m.Id == 30581601) { hasSnatchy = true; break; }
                    }
                    bool hasTarget = false;
                    foreach (var g in Bot.Graveyard)
                    {
                        if (g != null && (g.Id == 31425736 || g.Id == 10966439 || g.Id == 4215180 || g.Id == 68810435))
                        {
                            hasTarget = true;
                            break;
                        }
                    }
                    if (hasSnatchy && hasTarget)
                    {
                        LogToTurn("Activating Yummyusment☆Mignon effect to revive Level 1 from GY.");
                        return true;
                    }
                }
            }

            // 5. Yummyusment★Acroquey (93360904) - Field Spell
            if (card.Id == 93360904 && type == ExecutorType.Activate)
            {
                if (card.Location == CardLocation.Hand)
                {
                    LogToTurn("Activating Yummyusment★Acroquey from hand.");
                    return true;
                }
            }

            // --- LEVEL 1 MAIN DECK YUMMY MONSTERS ---

            // Normal Summoning Level 1 Yummys
            if (type == ExecutorType.Summon && (card.Id == 31425736 || card.Id == 10966439 || card.Id == 4215180 || card.Id == 68810435))
            {
                LogToTurn("Normal Summoning Level 1 Yummy: " + GetCardName(card.Id));
                return true;
            }

            // Special Summoning Level 1 Yummys
            if (type == ExecutorType.SpSummon && card.Location == CardLocation.Hand)
            {
                // Marshmao☆Yummy (10966439) can summon itself if we control no monsters or all are LIGHT Beast
                if (card.Id == 10966439)
                {
                    int botMonsters = GetZoneCount(Bot.MonsterZone);
                    bool allLightBeast = true;
                    if (botMonsters > 0)
                    {
                        foreach (var m in Bot.GetMonsters())
                        {
                            if (m != null && (!m.IsFaceup() || m.Race != (int)CardRace.Beast || m.Attribute != (int)CardAttribute.Light))
                            {
                                allLightBeast = false;
                                break;
                            }
                        }
                    }
                    if (botMonsters == 0 || allLightBeast)
                    {
                        LogToTurn("Special Summoning Marshmao☆Yummy from hand.");
                        return true;
                    }
                }

                // Cupsy☆Yummy (31425736), Lollipo☆Yummy (4215180), Cooky☆Yummy (68810435) can summon if we control Link-1 or Level 2 Synchro
                if (card.Id == 31425736 || card.Id == 4215180 || card.Id == 68810435)
                {
                    bool hasEnabler = false;
                    foreach (var m in Bot.GetMonsters())
                    {
                        if (m != null && m.IsFaceup() && (m.Id == 30581601 || m.Id == 31603289 || m.Id == 67098897 || m.Id == 93192592))
                        {
                            hasEnabler = true;
                            break;
                        }
                    }
                    if (hasEnabler)
                    {
                        LogToTurn("Special Summoning Level 1 Yummy from hand (enabler present): " + GetCardName(card.Id));
                        return true;
                    }
                }
            }

            // On-Summon trigger effects of Level 1 Yummys
            if (type == ExecutorType.Activate && (card.Id == 31425736 || card.Id == 10966439 || card.Id == 4215180 || card.Id == 68810435))
            {
                LogToTurn("Activating summon effect of Level 1 Yummy: " + GetCardName(card.Id));
                return true;
            }

            // --- LEVEL 2 SYNCHRO YUMMY WAY MONSTERS ---

            // Synchro Summons
            if (type == ExecutorType.SpSummon && (card.Id == 31603289 || card.Id == 67098897 || card.Id == 93192592))
            {
                LogToTurn("Synchro Summoning Level 2 Synchro: " + GetCardName(card.Id));
                return true;
            }

            // Synchro Summon effects and Tag-outs
            if (type == ExecutorType.Activate && (card.Id == 31603289 || card.Id == 67098897 || card.Id == 93192592))
            {
                if (Duel.Player == 1) // Opponent's turn: Tag-out
                {
                    LogToTurn("Yummy Synchro Tag-Out triggered on opponent's turn: returning to Extra Deck to revive 2 Yummys from GY.");
                    return true;
                }
                // Our turn: On-summon trigger search/draw/flip
                LogToTurn("Activating Level 2 Synchro summon effect: " + GetCardName(card.Id));
                return true;
            }

            // --- YUMMY☆SURPRISE ---

            if (type == ExecutorType.Activate && card.Id == 29369059)
            {
                LogToTurn("Activating Yummy☆Surprise.");
                return true;
            }

            // --- GENERIC EXTRA DECK payOFFS & STAPLES ---

            // Chaos Angel (22850702)
            if (type == ExecutorType.SpSummon && card.Id == 22850702)
            {
                LogToTurn("Synchro Summoning Chaos Angel.");
                return true;
            }
            if (type == ExecutorType.Activate && card.Id == 22850702)
            {
                LogToTurn("Activating Chaos Angel on-summon banish effect.");
                return true;
            }

            // S:P Little Knight (29301450)
            if (type == ExecutorType.SpSummon && card.Id == 29301450)
            {
                LogToTurn("Special Summoning S:P Little Knight.");
                return true;
            }
            if (type == ExecutorType.Activate && card.Id == 29301450)
            {
                LogToTurn("Activating S:P Little Knight effect.");
                return true;
            }

            // Linkuriboh (41999284) / Almiraj (60303245)
            if (type == ExecutorType.SpSummon && (card.Id == 41999284 || card.Id == 60303245))
            {
                LogToTurn("Special Summoning generic Link-1: " + GetCardName(card.Id));
                return true;
            }
            if (type == ExecutorType.Activate && (card.Id == 41999284 || card.Id == 60303245))
            {
                LogToTurn("Activating generic Link-1 effect: " + GetCardName(card.Id));
                return true;
            }

            return base.EvaluateCardAction(card, meta, type);
        }

        // Custom Card Selection Logic (Search / Discard / Fusion/Link/Synchro Materials)
        public override IList<ClientCard> OnSelectCard(IList<ClientCard> cards, int min, int max, long hint, bool cancelable)
        {
            if (cards == null || cards.Count == 0)
                return base.OnSelectCard(cards, min, max, hint, cancelable);

            // 1. Discard Selection (e.g. Cupsy★Yummy Way search cost)
            if (hint == 501) // Discard
            {
                // Prefer discarding Marshmao☆Yummy (10966439) since it can be easily revived or recycled
                foreach (var c in cards)
                {
                    if (c.Id == 10966439)
                    {
                        LogToTurn("OnSelectCard (Discard): Prioritizing discarding Marshmao☆Yummy.");
                        return new List<ClientCard> { c };
                    }
                }
                // Next prefer discarding duplicate Field Spells
                foreach (var c in cards)
                {
                    if (c.Id == 66975205 || c.Id == 93360904)
                    {
                        LogToTurn("OnSelectCard (Discard): Prioritizing discarding duplicate Field Spell: " + GetCardName(c.Id));
                        return new List<ClientCard> { c };
                    }
                }
            }

            // 2. Add to Hand Selection (e.g. Cupsy☆Yummy or Cupsy★Yummy Way search)
            if (hint == 506) // AddToHand
            {
                List<ClientCard> selected = new List<ClientCard>();

                // First priority: search Cupsy☆Yummy (31425736) if not already in hand/field
                bool hasCupsy = false;
                foreach (var h in Bot.Hand) if (h != null && h.Id == 31425736) hasCupsy = true;
                foreach (var m in Bot.GetMonsters()) if (m != null && m.Id == 31425736) hasCupsy = true;

                if (!hasCupsy)
                {
                    foreach (var c in cards)
                    {
                        if (c.Id == 31425736)
                        {
                            selected.Add(c);
                            if (selected.Count >= max) break;
                        }
                    }
                }

                // Second priority: search Marshmao☆Yummy (10966439)
                if (selected.Count < max)
                {
                    foreach (var c in cards)
                    {
                        if (c.Id == 10966439 && !selected.Contains(c))
                        {
                            selected.Add(c);
                            if (selected.Count >= max) break;
                        }
                    }
                }

                // Third priority: search Cooky☆Yummy (68810435) or Lollipo☆Yummy (4215180)
                if (selected.Count < max)
                {
                    foreach (var c in cards)
                    {
                        if ((c.Id == 68810435 || c.Id == 4215180) && !selected.Contains(c))
                        {
                            selected.Add(c);
                            if (selected.Count >= max) break;
                        }
                    }
                }

                // Fourth priority: search Yummy☆Surprise (29369059)
                if (selected.Count < max)
                {
                    foreach (var c in cards)
                    {
                        if (c.Id == 29369059 && !selected.Contains(c))
                        {
                            selected.Add(c);
                            if (selected.Count >= max) break;
                        }
                    }
                }

                if (selected.Count >= min)
                {
                    LogToTurn("OnSelectCard (Search): Selecting search targets.");
                    return selected;
                }
            }

            // 3. Link Summon Materials for Yummy★Snatchy (30581601)
            if (hint == 533) // LinkMaterial
            {
                // We want to link away a Level 1 Yummy monster on the field
                foreach (var c in cards)
                {
                    if (c.Location == CardLocation.MonsterZone && (c.Id == 31425736 || c.Id == 10966439 || c.Id == 4215180 || c.Id == 68810435))
                    {
                        LogToTurn("OnSelectCard (Link Material): Selecting " + GetCardName(c.Id) + " for Yummy★Snatchy Link Summon.");
                        return new List<ClientCard> { c };
                    }
                }
            }

            // 4. Return to Hand Selection (e.g. Yummy☆Surprise bounce effect)
            if (hint == 505) // ReturnToHand
            {
                List<ClientCard> selected = new List<ClientCard>();
                // Choose our own Level 1 monsters first (to recycle them)
                foreach (var c in cards)
                {
                    if (c.Controller == 0 && c.Location == CardLocation.MonsterZone && (c.Id == 31425736 || c.Id == 10966439 || c.Id == 4215180 || c.Id == 68810435))
                    {
                        selected.Add(c);
                    }
                }
                // Choose opponent's most dangerous cards
                List<ClientCard> enemyCards = new List<ClientCard>();
                foreach (var c in cards)
                {
                    if (c.Controller == 1)
                    {
                        enemyCards.Add(c);
                    }
                }
                // Sort enemy cards by calculated danger descending
                enemyCards.Sort((x, y) => CalculateCardDanger(y).CompareTo(CalculateCardDanger(x)));
                selected.AddRange(enemyCards);

                // Check count requirements
                if (selected.Count >= min)
                {
                    List<ClientCard> result = new List<ClientCard>();
                    for (int i = 0; i < Math.Min(max, selected.Count); i++)
                    {
                        result.Add(selected[i]);
                    }
                    LogToTurn("OnSelectCard (Bounce): Bouncing selected targets.");
                    return result;
                }
            }

            return base.OnSelectCard(cards, min, max, hint, cancelable);
        }

        // Custom Option Selection for Yummy☆Surprise (29369059)
        public override int OnSelectOption(IList<long> options)
        {
            if (Card != null && Card.Id == 29369059) // Yummy☆Surprise
            {
                // Option 0: Bounce 2 LIGHT Beasts we control and 2 opponent cards.
                // Option 1: Special Summon 1 "Yummy" from hand or GY.
                // Option 2: Recycle Field Spell.
                
                // If it is opponent's turn, we control 2+ LIGHT Beasts, and opponent has 2+ cards: choose Option 0 (Bounce)
                if (Duel.Player == 1 && options.Count > 0)
                {
                    int ourBeasts = 0;
                    foreach (var m in Bot.GetMonsters())
                    {
                        if (m != null && m.IsFaceup() && m.Race == (int)CardRace.Beast && m.Attribute == (int)CardAttribute.Light)
                            ourBeasts++;
                    }
                    int enemyCards = Enemy.GetMonsterCount() + GetZoneCount(Enemy.SpellZone);

                    if (ourBeasts >= 2 && enemyCards >= 2)
                    {
                        LogToTurn("OnSelectOption (Yummy☆Surprise): Selecting Option 0 (Bounce) to disrupt opponent.");
                        return 0;
                    }
                }

                // Default: Special Summon (Option 1) to extend combo
                if (options.Count > 1)
                {
                    LogToTurn("OnSelectOption (Yummy☆Surprise): Selecting Option 1 (Special Summon) for combo extension.");
                    return 1;
                }
            }

            return base.OnSelectOption(options);
        }

        // Always accept optional triggers from Yummy cards
        public override bool OnSelectYesNo(long desc)
        {
            LogToTurn("OnSelectYesNo: Automatically accepting optional trigger prompt (desc ID: " + desc + ").");
            return true;
        }
    }
}
