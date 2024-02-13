using MonoMod.Cil;
using Mono.Cecil.Cil;
using Terraria.ModLoader;
using System.Reflection;
using System;
using System.Collections.Generic;
using CalamityMod.Items;
using MonoMod.RuntimeDetour;
using Terraria;
using Terraria.ID;

namespace RemoveCalamitySummonerNerf
{
	public class RemoveCalamitySummonerNerf : Mod
	
	{
		private Mod Calamity => ModLoader.GetMod("CalamityMod");
		private static MethodInfo ModifyHitNPCWithProj = null;
		private static MethodInfo UpdateEquip = null;
		private static MethodInfo UpdateArmorSet = null;
		private static ILHook damageNerfHook = null;
		private static ILHook valhallaItemHook = null;
		private static ILHook valhallaSetHook = null;
		public override void Load()
		{
			if (Calamity == null)
				return;
			Type CalamityPlayer = null;
			Type CalamityGlobalItem = null;
			Assembly CalamityAssembly = Calamity.GetType().Assembly;
			foreach (Type t in CalamityAssembly.GetTypes())
			{
				if (t.Name == "CalamityPlayer")
					CalamityPlayer = t;

				if (t.Name == "CalamityGlobalItem")
					CalamityGlobalItem = t;
			}

			if (CalamityPlayer == null)
			{
				Logger.Error("CalamityPlayer is null");
				return;
			}

			if (CalamityGlobalItem == null)
			{
				Logger.Error("CalamityGlobalItem is null");
				return;
			}
			ModifyHitNPCWithProj = CalamityPlayer.GetMethod("ModifyHitNPCWithProj", BindingFlags.Public | BindingFlags.Instance);
			UpdateEquip = CalamityGlobalItem.GetMethod("UpdateEquip", BindingFlags.Public | BindingFlags.Instance);
			UpdateArmorSet = CalamityGlobalItem.GetMethod("UpdateArmorSet", BindingFlags.Public | BindingFlags.Instance);
			if (ModifyHitNPCWithProj == null)
			{
				Logger.Error("Could not find ModifyHitNPCWithProj");
				return;
			}if (UpdateEquip == null)
			{
				Logger.Error("Could not find UpdateEquip");
				return;
			}if (UpdateArmorSet == null)
			{
				Logger.Error("Could not find UpdateArmorSet");
				return;
			}
			damageNerfHook=new ILHook(ModifyHitNPCWithProj, SummonNerfHook);
			damageNerfHook.Apply();
			valhallaItemHook = new ILHook(UpdateEquip, ValhallaNerfHook);
			valhallaItemHook.Apply();
			valhallaSetHook = new ILHook(UpdateArmorSet, ValhallaSetHook);
			valhallaSetHook.Apply();
		}
		
		private void SummonNerfHook(ILContext il)
		{
			var c = new ILCursor(il);
			if(!c.TryGotoNext(i=>i.MatchLdloc(1)))
			{
				Logger.Error("Could not find Ldloc for summon damage nerf");
				return;
			}
			c.Remove(); //remove the summon damage type check
			c.Emit(OpCodes.Ldc_I4_0); //replace the missing stack value with 0 (false)

		}

		private void ValhallaSetHook(ILContext il)
		{
			var c = new ILCursor(il);
			if (!c.TryGotoNext(i => i.MatchLdcI4(6)))
			{
				Logger.Error("Could not find LdcI4 for Valhalla Set Regen Buff");
				return;
			}
			c.Remove(); // remove the regen buff
			c.Emit(OpCodes.Ldc_I4_0); //change it to 0 instead
			if (!c.TryGotoNext(i => i.MatchLdcR4(0.1f)))
			{
				Logger.Error("Could not find LdcR4 for Valhalla set Summon Damage Buff");
				return;
			}
			c.Remove(); // remove the summon damage buff
			c.Emit(OpCodes.Ldc_R4, 0.0f); // replace it with 0 instead
			if (!c.TryGotoNext(i => i.MatchLdcR4(10f)))
			{
				Logger.Error("Could not find LdcR4 for Valhalla set Melee Crit Chance Buff");
				return;
			}
			c.Remove(); // remove the melee crit chance buff
			c.Emit(OpCodes.Ldc_R4, 0.0f); // replace it with 0 instead
			
				if (!c.TryGotoNext(i => i.MatchLdstr("Vanilla.Armor.SetBonus.SquireTier3")))
				{
					Logger.Error("Could not find Ldstr for Valhalla Set Bonus Text");
					return;
				}

				c.GotoPrev();
			c.RemoveRange(4);
		}

		private void ValhallaNerfHook(ILContext il)
		{
			var c = new ILCursor(il);
			if (!c.TryGotoNext(i => i.MatchLdcI4(6)))
			{
				Logger.Error("Could not find LdcI4 for Valhalla chestplate Life Regen nerf");
				return;
			}
			c.Remove(); // remove the regen nerf
			c.Emit(OpCodes.Ldc_I4_0); //change it to 0 instead
			if (!c.TryGotoNext(i => i.MatchLdcR4(0.1f)))
			{
				Logger.Error("Could not find LdcR4 for Valhalla leggings Summon Damage nerf");
				return;
			}
			c.Remove(); // remove the summon damage nerf
			c.Emit(OpCodes.Ldc_R4, 0.0f); // replace it with 0 instead
			if (!c.TryGotoNext(i => i.MatchLdcR4(10f)))
			{
				Logger.Error("Could not find LdcR4 for Valhalla Leggings Melee Crit Chance nerf");
				return;
			}
			c.Remove(); // remove the melee crit chance nerf
			c.Emit(OpCodes.Ldc_R4, 0.0f); // replace it with 0 instead

		}

		

		
	}

	public class ValhallaChestplate : GlobalItem
	{
		public override bool AppliesToEntity(Item entity, bool lateInstantiation)
		{
			return entity.type == ItemID.SquireAltShirt;
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			foreach (TooltipLine line in tooltips)
			{
				if (line.Name == "Tooltip0")
				{
					line.Text = "30% increased minion damage and massively increased life regeneration";
				}
			}
		}	
	}
	public class ValhallaPants : GlobalItem
	{
		public override bool AppliesToEntity(Item entity, bool lateInstantiation)
		{
			return entity.type == ItemID.SquireAltPants;
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			foreach (TooltipLine line in tooltips)
			{
				if (line.Name == "Tooltip0")
				{
					line.Text = "20% increased minion damage and melee critical strike chance";
				}
			}
		}	
	}
}