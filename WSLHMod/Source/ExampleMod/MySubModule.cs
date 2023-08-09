using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.Library;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.SaveSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party;
using HarmonyLib;
using static TaleWorlds.CampaignSystem.Party.MobileParty;
using System.Runtime;
using MCM.Abstractions.GameFeatures;
using MCM.Abstractions.FluentBuilder;
using MCM.Abstractions;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Base;
using MCM.Abstractions.Properties;
using MCM.Abstractions.Global;
using MCM.Abstractions.Xml;
using MCM.Implementation;
using System.Xml.Xsl;
using MCM.Common;
using MCM.Abstractions.Base.Global;


namespace WSLHMod
{
    public class MySubModule : MBSubModuleBase
    {
        // MCM Menu settings
        public bool modSetup = false;
        public static readonly string ModuleFolderName = "WSHLHMod";
        public static readonly string ModName = "WSHLHMod";
        private ConversationManager conversation_manager = null;
        private CampaignGameStarter cgs = null;
        private Agent _agent; // Store the agent reference here when initiating the dialogue
        //private static LHSettings _instance;

        public bool InitialDialogueCheck()
        {
            
            if (Hero.OneToOneConversationHero == null && Hero.OneToOneConversationHero.IsFugitive == false && Hero.OneToOneConversationHero.IsHumanPlayerCharacter == false) { return false; }
            return true;
        }

        private bool conversation_is_clan_member_not_in_party_on_condition()
        {
            return Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero.Clan == Hero.MainHero.Clan && Hero.MainHero.PartyBelongedTo != Hero.OneToOneConversationHero.PartyBelongedTo;
        }

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            //CampaignEvents.OnGameLoadedEvent
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            if (game.GameType is Campaign)
            {
                // Subscribe to the CampaignEvents.OnGameLoadedEvent event

                CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);

            }
        }

        private void OnGameLoaded(CampaignGameStarter campaignGameStarter)
        {
            //Loads in the adventure map hopefully.
            AddDialogs(campaignGameStarter);
            InformationManager.DisplayMessage(new InformationMessage("Love/Hate mod loaded"));
        }

        protected void AddDialogs(CampaignGameStarter starter)
        {
            TextObject WS_DIAG1 = new TextObject("You and I need to talk...");
            TextObject WS_DIAG2 = new TextObject("What do you want?");
            TextObject WS_DIAG3 = new TextObject("I want you to love me.");
            TextObject WS_DIAG4 = new TextObject("I want you to like me.");
            TextObject WS_DIAG5 = new TextObject("I want our relationship to be neutral.");
            TextObject WS_DIAG6 = new TextObject("I want you to dislike me.");
            TextObject WS_DIAG7 = new TextObject("I want you to hate me.");
            TextObject WS_DIAG8 = new TextObject("Never mind.");
            TextObject WS_DIAG9 = new TextObject("I like you. (+10 to relations)");
            TextObject WS_DIAG10 = new TextObject("I dislike you (-10 to relations)");
            TextObject WS_DIAG11 = new TextObject("I love you!");
            TextObject WS_DIAG12 = new TextObject("I hate you!");
            TextObject WS_DIAG13 = new TextObject("I feel neutral towards you.");

            TextObject WS_DIAG14 = new TextObject("Very well.");
            TextObject WS_DIAG15 = new TextObject("I cannot");

            starter.AddPlayerLine("WSLH_orders_begin", "hero_main_options", "WSLH_orders_list", WS_DIAG1.ToString(), InitialDialogueCheck, null, 66, null, null);
            starter.AddDialogLine("WSLH_orders_list", "WSLH_orders_list", "WSLH_Response1", WS_DIAG2.ToString(), null, null, 100, null);
            starter.AddPlayerLine("WSLH_ask_love", "WSLH_Response1", "WSLH_npc_love", WS_DIAG3.ToString(), null, null, 70, null);
            starter.AddPlayerLine("WSLH_ask_like", "WSLH_Response1", "WSLH_npc_like", WS_DIAG4.ToString(), null, null, 69, null);
            starter.AddPlayerLine("WSLH_ask_neutral", "WSLH_Response1", "WSLH_npc_neutral", WS_DIAG5.ToString(), null, null, 68, null);
            starter.AddPlayerLine("WSLH_ask_dislike", "WSLH_Response1", "WSLH_npc_dislike", WS_DIAG6.ToString(), null, null, 67, null);
            starter.AddPlayerLine("WSLH_ask_hate", "WSLH_Response1", "WSLH_npc_hate", WS_DIAG7.ToString(), null, null, 66, null);
            starter.AddPlayerLine("WSLH_goback", "WSLH_Response1", "WSLH_npc_ignore", WS_DIAG8.ToString(), null, null, 65, null);
            
            starter.AddDialogLine("WSLH_orders_comply1", "WSLH_npc_love", "WSLH_orders_exit", WS_DIAG11.ToString(), null, () => LoveWithPlayer(Hero.OneToOneConversationHero), 100, null);
            starter.AddDialogLine("WSLH_orders_comply2", "WSLH_npc_like", "WSLH_orders_exit", WS_DIAG9.ToString(), null, () => IncreaseRelationsWithPlayer(Hero.OneToOneConversationHero, 10), 90, null);
            starter.AddDialogLine("WSLH_orders_comply3", "WSLH_npc_neutral", "WSLH_orders_exit", WS_DIAG13.ToString(), null, () => NeutralTowardPlayer(Hero.OneToOneConversationHero), 80, null);
            starter.AddDialogLine("WSLH_orders_comply4", "WSLH_npc_dislike", "WSLH_orders_exit", WS_DIAG10.ToString(), null, () => DecreaseRelationsWithPlayer(Hero.OneToOneConversationHero, 10), 70, null);
            starter.AddDialogLine("WSLH_orders_comply5", "WSLH_npc_hate", "WSLH_orders_exit", WS_DIAG12.ToString(), null, () => HateAgainstPlayer(Hero.OneToOneConversationHero), 66, null);           
            starter.AddDialogLine("WSLH_orders_comply6", "WSLH_npc_ignore", "WSLH_orders_exit", WS_DIAG15.ToString(), null, null, 65, null);
        }

        private void SetPartyBehavior(Hero npcHero, PartyObjective partyObjective)
        {
            if (npcHero != null)
            {
                MobileParty npcParty = npcHero.PartyBelongedTo;

                if (npcParty != null && npcParty != MobileParty.MainParty)
                {
                    npcParty.SetPartyObjective(partyObjective);
                }
            }
        }

        public void LoveWithPlayer(Hero hero)
        {
            if (hero != null)
            {
                int currentRelation = hero.GetRelation(Hero.MainHero);
                if (currentRelation != 100) { hero.SetPersonalRelation(Hero.MainHero, 100); }
            }
        }

        public void NeutralTowardPlayer(Hero hero)
        {
            if (hero != null)
            {
                int currentRelation = hero.GetRelation(Hero.MainHero);
                if (currentRelation != 0) { hero.SetPersonalRelation(Hero.MainHero, 0); }
            }
        }

        public void GiveEntityGold(Hero hero)
        {
            if (hero != null)
            {

            }
        }

        public void HateAgainstPlayer(Hero hero)
        {
            if (hero != null)
            {
                int currentRelation = hero.GetRelation(Hero.MainHero);
                if (currentRelation != -99) { hero.SetPersonalRelation(Hero.MainHero, -99); }
            }
        }

        public void IncreaseRelationsWithPlayer(Hero hero, int amount)
        {
            // Check if the hero is a valid character and is not the player
            if (hero != null)
            {
                // Get the current relation value between the hero and the player
                int currentRelation = hero.GetRelation(Hero.MainHero);
                if (currentRelation <= 90)
                {
                    currentRelation = currentRelation + amount;
                    hero.SetPersonalRelation(Hero.MainHero, currentRelation);
                }
            }
        }

        public void DecreaseRelationsWithPlayer(Hero hero, int amount)
        {
            // Check if the hero is a valid character and is not the player
            if (hero != null)
            {
                // Get the current relation value between the hero and the player
                int currentRelation = hero.GetRelation(Hero.MainHero);
                if (currentRelation >= -90)
                {
                    currentRelation = currentRelation - amount;
                    hero.SetPersonalRelation(Hero.MainHero, currentRelation);
                }
            }
        }

        private bool CanShowDialogueOption()
        {
            // Add conditions to check if the player is interacting with a clan member
            CharacterObject speakingCharacter = CharacterObject.OneToOneConversationCharacter;
            return speakingCharacter != null && speakingCharacter.IsHero;
        }

        private void OnDialogueOptionSelected()
        {
            // Handle what happens when the player selects this dialogue option
            // For example, you can trigger another dialogue or perform an action.
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
            if (modSetup != true)
            {
                modSetup = true;
                WSLHSettings.Instance.Settings();
                //InformationManager.DisplayMessage(new InformationMessage("Love/Hate mod loaded."));
            }
        }
    }

    public class WSLHSettings : IDisposable
    {
        private static WSLHSettings _instance;
        private FluentGlobalSettings globalSettings;

        public void Dispose()
        {
            //NKSettings.Unregister();
        }


        public static WSLHSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WSLHSettings();
                }
                return _instance;
            }
        }


        public void Settings()
        {

        }
    }
}