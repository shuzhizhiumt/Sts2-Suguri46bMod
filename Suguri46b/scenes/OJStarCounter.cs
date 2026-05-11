using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace Suguri46b.Suguri46b.scenes;
public partial class OJStarCounter : Control  
{
	private HoverTip _hoverTip;
	public override void _Ready()
	{
		_hoverTip = new HoverTip(new LocString("static_hover_tips","OJSTAR_COUNTER.title"),new LocString("static_hover_tips","OJSTAR_COUNTER.description"),null);
		Connect(Control.SignalName.MouseEntered,Callable.From(OnHovered));
		Connect(Control.SignalName.MouseExited,Callable.From(OnUnhovered));
	}
	private void OnHovered()
	{
		NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this,_hoverTip);
		nHoverTipSet.GlobalPosition = GlobalPosition + new Vector2(-70f,-150f);
	}
	private void OnUnhovered()
	{
		NHoverTipSet.Remove(this);
	}
		public override void _Process(double delta)
	{
	}
}
