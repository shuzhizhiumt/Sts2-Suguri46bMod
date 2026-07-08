// using HarmonyLib;
// using MegaCrit.Sts2.Core.CardSelection;
// using MegaCrit.Sts2.Core.Commands;
// using MegaCrit.Sts2.Core.Entities.Cards;
// using MegaCrit.Sts2.Core.Entities.Gold;
// using MegaCrit.Sts2.Core.Events;
// using MegaCrit.Sts2.Core.GameActions.Multiplayer;
// using MegaCrit.Sts2.Core.Helpers;
// using MegaCrit.Sts2.Core.Localization;
// using MegaCrit.Sts2.Core.Localization.DynamicVars;
// using MegaCrit.Sts2.Core.Models;
// using MegaCrit.Sts2.Core.Models.Acts;
// using MegaCrit.Sts2.Core.Models.Cards;
// using MegaCrit.Sts2.Core.Rewards;
// using MegaCrit.Sts2.Core.Runs;
// using MegaCrit.Sts2.Core.ValueProps;
// using STS2RitsuLib.CardTags;
// using STS2RitsuLib.Interop.AutoRegistration;
// using STS2RitsuLib.Keywords;
// using STS2RitsuLib.Scaffolding.Content;
// using Suguri46b.Scripts.CardKeyWords;
// using Suguri46b.Scripts.Cards.Token;

// namespace Suguri46b.Scripts.Event;

// [RegisterSharedEvent] // 指定只有荣耀这章生成
// // [RegisterSharedEvent] // 如果需要自定义生成条件，可以注册成通用再重载isAllowed
// public sealed class Star_Blasting_FuseEvent : ModEventTemplate
// {
//     // 背景图位置
//     public override EventAssetProfile AssetProfile => new(
//         InitialPortraitPath: "res://images/events/battleworn_dummy.png"
//     );

//     // 设置一些数值
//     protected override IEnumerable<DynamicVar> CanonicalVars =>
//     [
//         new DamageVar(10m, ValueProp.Unblockable | ValueProp.Unpowered),
//         new CardsVar(5)
//     ];

//     // 什么时候会遇到。
//     public override bool IsAllowed(IRunState runState) => runState.Players.All(p =>p.Piles.Count()>DynamicVars.Cards.IntValue);

//     // 生成事件初始选项。这里是两个选项：失去生命值或者失去金币，然后进入选择奖励阶段
//     // 与 CustomEventModel.Option(delegate, pageKey) 一致：textKey = Id.Entry + ".pages." + page + ".options." + Slugify(方法名)
//     protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
//     [
//         new EventOption(this, AddTagsInvisibleBomb, InitialOptionKey("ADD_TAGS_INVISIBLE_BOMB"))
//     ];
//     private async Task AddTagsInvisibleBomb()
//     {
//         var selectedCards = await CardSelectCmd.FromHand(
//             prefs: new CardSelectorPrefs(new LocString("card_selection", "ADD_INVISIBLE_BOMB"), 3, DynamicVars.Cards.IntValue),
//             context: new BlockingPlayerChoiceContext(),
//             player: Owner,
//             filter: card => !card.Tags.Any(t => t == MyTags.Invisible_Bomb),
//             source: this);
//         foreach (var card in selectedCards)
//         {
//             card?.Tags.AddItem(MyTags.Invisible_Bomb);
//         }
// 		await AddAndPreview<Exterminate>(L10NLookup($"{Id.Entry}.pages.ADD_TAGS.description"));
//     }
//     private async Task AddAndPreview<T>(LocString loc) where T : CardModel
// 	{
// 		CardModel card = base.Owner.RunState.CreateCard<T>(base.Owner);
// 		CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck), 1f);
// 		SetEventFinished(loc);
// 	}
// }