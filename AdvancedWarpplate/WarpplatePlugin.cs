using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;
namespace PluginTemplate
{
	[ApiVersion(1, 16)]
	public class WarpplatePlugin : TerrariaPlugin
	{
		public class Player
		{
			public int Index
			{
				get;
				set;
			}
			public TSPlayer TSPlayer
			{
				get
				{
					return TShock.Players[this.Index];
				}
			}
			public int warpplatetime
			{
				get;
				set;
			}
			public bool warpplateuse
			{
				get;
				set;
			}
			public bool warped
			{
				get;
				set;
			}
			public int warpcooldown
			{
				get;
				set;
			}
			public Player(int index)
			{
				this.Index = index;
				this.warpplatetime = 0;
				this.warpplateuse = true;
				this.warped = false;
				this.warpcooldown = 0;
			}
		}
		public static List<WarpplatePlugin.Player> Players = new List<WarpplatePlugin.Player>();
		public static WarpplateManager Warpplates;
		private DateTime LastCheck = DateTime.UtcNow;
		public override string Name
		{
			get
			{
				return "Warpplate";
			}
		}
		public override string Author
		{
			get
			{
				return "Created by DarkunderdoG, modified by 2.0";
			}
		}
		public override string Description
		{
			get
			{
				return "Warpplate";
			}
		}
		public override Version Version
		{
			get
			{
				return Assembly.GetExecutingAssembly().GetName().Version;
			}
		}
		public override void Initialize()
		{
			WarpplatePlugin.Warpplates = new WarpplateManager(TShock.DB);
			HookManager hooks = ServerApi.Hooks;
			hooks.GamePostInitialize.Register(this, delegate(EventArgs args)
			{
				this.OnPostInit();
			});
			hooks.GameInitialize.Register(this, delegate(EventArgs args)
			{
				this.OnInitialize();
			});
			hooks.GameUpdate.Register(this, delegate(EventArgs args)
			{
				this.OnUpdate();
			});
			hooks.NetGreetPlayer.Register(this, this.OnGreetPlayer);
			hooks.ServerLeave.Register(this, this.OnLeave);
		}
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				HookManager hooks = ServerApi.Hooks;
				hooks.GamePostInitialize.Deregister(this, delegate(EventArgs args)
				{
					this.OnPostInit();
				});
				hooks.GameInitialize.Deregister(this, delegate(EventArgs args)
				{
					this.OnInitialize();
				});
				hooks.GameUpdate.Deregister(this, delegate(EventArgs args)
				{
					this.OnUpdate();
				});
				hooks.NetGreetPlayer.Deregister(this, this.OnGreetPlayer);
				hooks.ServerLeave.Deregister(this, this.OnLeave);
			}
			base.Dispose(disposing);
		}
		private void OnPostInit()
		{
			WarpplatePlugin.Warpplates.ReloadAllWarpplates();
		}
		public WarpplatePlugin(Main game) : base(game)
		{
			base.Order = 1;
		}
		public void OnInitialize()
		{
			Commands.ChatCommands.Add(new Command("setwarpplate", new CommandDelegate(WarpplatePlugin.setwarpplate), new string[]
			{
				"swp"
			}));
			Commands.ChatCommands.Add(new Command("setwarpplate", new CommandDelegate(WarpplatePlugin.delwarpplate), new string[]
			{
				"dwp"
			}));
			Commands.ChatCommands.Add(new Command("setwarpplate", new CommandDelegate(WarpplatePlugin.warpplatedest), new string[]
			{
				"swpd"
			}));
			Commands.ChatCommands.Add(new Command("setwarpplate", new CommandDelegate(WarpplatePlugin.removeplatedest), new string[]
			{
				"rwpd"
			}));
			Commands.ChatCommands.Add(new Command("setwarpplate", new CommandDelegate(WarpplatePlugin.wpi), new string[]
			{
				"wpi"
			}));
			Commands.ChatCommands.Add(new Command("setwarpplate", new CommandDelegate(WarpplatePlugin.warpallow), new string[]
			{
				"wpallow"
			}));
			Commands.ChatCommands.Add(new Command("setwarpplate", new CommandDelegate(WarpplatePlugin.reloadwarp), new string[]
			{
				"reloadwarp"
			}));
			Commands.ChatCommands.Add(new Command("setwarpplate", new CommandDelegate(WarpplatePlugin.setwarpplatedelay), new string[]
			{
				"swpdl"
			}));
			Commands.ChatCommands.Add(new Command("setwarpplate", new CommandDelegate(WarpplatePlugin.setwarpplatewidth), new string[]
			{
				"swpw"
			}));
			Commands.ChatCommands.Add(new Command("setwarpplate", new CommandDelegate(WarpplatePlugin.setwarpplateheight), new string[]
			{
				"swph"
			}));
			Commands.ChatCommands.Add(new Command("setwarpplate", new CommandDelegate(WarpplatePlugin.setwarpplatesize), new string[]
			{
				"swps"
			}));
			Commands.ChatCommands.Add(new Command("setwarpplate", new CommandDelegate(WarpplatePlugin.setwarpplatelabel), new string[]
			{
				"swpl"
			}));
		}
		public void OnGreetPlayer(GreetPlayerEventArgs args)
		{
			lock (WarpplatePlugin.Players)
			{
				WarpplatePlugin.Players.Add(new WarpplatePlugin.Player(args.Who));
			}
		}
		private void OnUpdate()
		{
			if ((DateTime.UtcNow - this.LastCheck).TotalSeconds >= 1.0)
			{
				this.LastCheck = DateTime.UtcNow;
				lock (WarpplatePlugin.Players)
				{
					foreach (WarpplatePlugin.Player current in WarpplatePlugin.Players)
					{
						if (current != null && current.TSPlayer != null && current.TSPlayer.Group.HasPermission("warpplate") && current.warpplateuse)
						{
							if (current.warpcooldown != 0)
							{
								current.warpcooldown--;
							}
							else
							{
								string text = WarpplatePlugin.Warpplates.InAreaWarpplateName(current.TSPlayer.TileX, current.TSPlayer.TileY);
								if (text == null || text == "")
								{
									current.warpplatetime = 0;
									current.warped = false;
								}
								else
								{
									if (!current.warped)
									{
										Warpplate warpplate = WarpplatePlugin.Warpplates.FindWarpplate(text);
										string name = warpplate.Name;
										Warpplate warpplate2 = WarpplatePlugin.Warpplates.FindWarpplate(warpplate.WarpDest);
										string text2 = "";
										if (warpplate2 != null)
										{
											current.warpplatetime++;
											if (warpplate.Delay - current.warpplatetime > 0)
											{
												current.TSPlayer.SendMessage(string.Concat(new object[]
												{
													"You Will Be Warped To ",
													WarpplatePlugin.Warpplates.GetLabel(warpplate.WarpDest),
													" in ",
													warpplate.Delay - current.warpplatetime,
													" Seconds"
												}));
												break;
											}
											if (current.TSPlayer.Teleport((float)((int)(warpplate2.WarpplatePos.X * 16f) + 32), (float)((int)(warpplate2.WarpplatePos.Y * 16f)), 1))
											{
												current.TSPlayer.SendMessage("You Have Been Warped To " + WarpplatePlugin.Warpplates.GetLabel(warpplate.WarpDest) + " via a Warpplate");
											}
											if (name.Contains("Level 1") && current.TSPlayer.Group.Prefix == "NewBie")
											{
												text2 = "Level 1";
											}
											else
											{
												if (name.Contains("Level 2") && current.TSPlayer.Group.Prefix == "Level 1")
												{
													text2 = "Level 2";
												}
												else
												{
													if (name.Contains("Level 3") && current.TSPlayer.Group.Prefix == "Level 2")
													{
														text2 = "Level 3";
													}
												}
											}
											if (text2 != "")
											{
												List<TSPlayer> list = TShock.Utils.FindPlayer(current.TSPlayer.Name);
												List<Group> list2 = WarpplatePlugin.FindGroup(text2);
												TSPlayer tSPlayer = list[0];
												Group group = list2[0];
												User userByName = TShock.Users.GetUserByName(tSPlayer.UserAccountName);
												TShock.Users.SetUserGroup(userByName, group.Name);
												current.TSPlayer.SendMessage("Your Rank Has Been Upgraded To " + group.Name + " Via Warpplate!!", Color.Green);
												TSPlayer.All.SendInfoMessage(current.TSPlayer.Name + "'s Rank Has Been Upgraded To " + group.Name + " Via Warpplate Ranking");
												current.TSPlayer.Group = group;
											}
											current.warpplatetime = 0;
											current.warped = true;
											current.warpcooldown = 3;
										}
									}
								}
							}
						}
					}
				}
			}
		}
		private void OnLeave(LeaveEventArgs args)
		{
			lock (WarpplatePlugin.Players)
			{
				WarpplatePlugin.Players.RemoveAll((WarpplatePlugin.Player p) => p.Index == args.Who);
			}
		}
		private static int GetPlayerIndex(int ply)
		{
			int result;
			lock (WarpplatePlugin.Players)
			{
				int num = -1;
				for (int i = 0; i < WarpplatePlugin.Players.Count; i++)
				{
					if (WarpplatePlugin.Players[i].Index == ply)
					{
						num = i;
					}
				}
				result = num;
			}
			return result;
		}
		private static void setwarpplatedelay(CommandArgs args)
		{
			string name;
			if (args.Parameters.Count == 2)
			{
				name = args.Parameters[0];
			}
			else
			{
				if (args.Parameters.Count != 1)
				{
					args.Player.SendMessage("Invalid syntax! Proper syntax: /swpd [<warpplate name>] <delay in seconds>", Color.Red);
					args.Player.SendMessage("Set 0 for immediate warp", Color.Red);
					return;
				}
				name = WarpplatePlugin.Warpplates.InAreaWarpplateName(args.Player.TileX, args.Player.TileY);
			}
			Warpplate warpplate = WarpplatePlugin.Warpplates.FindWarpplate(name);
			if (warpplate == null)
			{
				args.Player.SendMessage("No such warpplate", Color.Red);
				return;
			}
			int num;
			if (!int.TryParse(args.Parameters[args.Parameters.Count - 1], out num))
			{
				args.Player.SendMessage("Bad number specified", Color.Red);
				return;
			}
			warpplate.Delay = num + 1;
			if (WarpplatePlugin.Warpplates.UpdateWarpplate(warpplate.Name))
			{
				args.Player.SendMessage(string.Format("Set delay of {0} to {1} seconds", warpplate.Name, num), Color.Green);
				return;
			}
			args.Player.SendMessage("Something went wrong", Color.Red);
		}
		private static void setwarpplatewidth(CommandArgs args)
		{
			string name;
			if (args.Parameters.Count == 2)
			{
				name = args.Parameters[0];
			}
			else
			{
				if (args.Parameters.Count != 1)
				{
					args.Player.SendMessage("Invalid syntax! Proper syntax: /swpw [<warpplate name>] <width in blocks>", Color.Red);
					return;
				}
				name = WarpplatePlugin.Warpplates.InAreaWarpplateName(args.Player.TileX, args.Player.TileY);
			}
			Warpplate warpplate = WarpplatePlugin.Warpplates.FindWarpplate(name);
			if (warpplate == null)
			{
				args.Player.SendMessage("No such warpplate", Color.Red);
				return;
			}
			int num;
			if (!int.TryParse(args.Parameters[args.Parameters.Count - 1], out num))
			{
				args.Player.SendMessage("Bad number specified", Color.Red);
				return;
			}
			Rectangle area = warpplate.Area;
			area.Width = num;
			warpplate.Area = area;
			if (WarpplatePlugin.Warpplates.UpdateWarpplate(warpplate.Name))
			{
				args.Player.SendMessage(string.Format("Set width of {0} to {1} blocks", warpplate.Name, num), Color.Green);
				return;
			}
			args.Player.SendMessage("Something went wrong", Color.Red);
		}
		private static void setwarpplatelabel(CommandArgs args)
		{
			string name;
			if (args.Parameters.Count == 2)
			{
				name = args.Parameters[0];
			}
			else
			{
				if (args.Parameters.Count != 1)
				{
					args.Player.SendMessage("Invalid syntax! Proper syntax: /swpl [<warpplate name>] <label>", Color.Red);
					args.Player.SendMessage("Type /swpl [<warpplate name>] \"\" to set label to default (warpplate name)", Color.Red);
					return;
				}
				name = WarpplatePlugin.Warpplates.InAreaWarpplateName(args.Player.TileX, args.Player.TileY);
			}
			Warpplate warpplate = WarpplatePlugin.Warpplates.FindWarpplate(name);
			if (warpplate == null)
			{
				args.Player.SendMessage("No such warpplate", Color.Red);
				return;
			}
			string label = args.Parameters[args.Parameters.Count - 1];
			warpplate.Label = label;
			if (WarpplatePlugin.Warpplates.UpdateWarpplate(warpplate.Name))
			{
				args.Player.SendMessage(string.Format("Set label of {0} to {1}", warpplate.Name, WarpplatePlugin.D(warpplate)), Color.Green);
				return;
			}
			args.Player.SendMessage("Something went wrong", Color.Red);
		}
		private static void setwarpplateheight(CommandArgs args)
		{
			string name;
			if (args.Parameters.Count == 2)
			{
				name = args.Parameters[0];
			}
			else
			{
				if (args.Parameters.Count != 1)
				{
					args.Player.SendMessage("Invalid syntax! Proper syntax: /swph [<warpplate name>] <height in blocks>", Color.Red);
					return;
				}
				name = WarpplatePlugin.Warpplates.InAreaWarpplateName(args.Player.TileX, args.Player.TileY);
			}
			Warpplate warpplate = WarpplatePlugin.Warpplates.FindWarpplate(name);
			if (warpplate == null)
			{
				args.Player.SendMessage("No such warpplate", Color.Red);
				return;
			}
			int num;
			if (!int.TryParse(args.Parameters[args.Parameters.Count - 1], out num))
			{
				args.Player.SendMessage("Bad number specified", Color.Red);
				return;
			}
			Rectangle area = warpplate.Area;
			area.Height = num;
			warpplate.Area = area;
			WarpplatePlugin.Warpplates.UpdateWarpplate(warpplate.Name);
			if (WarpplatePlugin.Warpplates.UpdateWarpplate(warpplate.Name))
			{
				args.Player.SendMessage(string.Format("Set height of {0} to {1} blocks", warpplate.Name, num), Color.Green);
				return;
			}
			args.Player.SendMessage("Something went wrong", Color.Red);
		}
		private static void setwarpplatesize(CommandArgs args)
		{
			string name;
			if (args.Parameters.Count == 3)
			{
				name = args.Parameters[0];
			}
			else
			{
				if (args.Parameters.Count != 2)
				{
					args.Player.SendMessage("Invalid syntax! Proper syntax: /swps [<warpplate name>] <width> <height>", Color.Red);
					return;
				}
				name = WarpplatePlugin.Warpplates.InAreaWarpplateName(args.Player.TileX, args.Player.TileY);
			}
			Warpplate warpplate = WarpplatePlugin.Warpplates.FindWarpplate(name);
			if (warpplate == null)
			{
				args.Player.SendMessage("No such warpplate", Color.Red);
				return;
			}
			int num;
			int num2;
			if (!int.TryParse(args.Parameters[args.Parameters.Count - 2], out num) || !int.TryParse(args.Parameters[args.Parameters.Count - 1], out num2))
			{
				args.Player.SendMessage("Bad number specified", Color.Red);
				return;
			}
			Rectangle area = warpplate.Area;
			area.Width = num;
			area.Height = num2;
			warpplate.Area = area;
			WarpplatePlugin.Warpplates.UpdateWarpplate(warpplate.Name);
			if (WarpplatePlugin.Warpplates.UpdateWarpplate(warpplate.Name))
			{
				args.Player.SendMessage(string.Format("Set size of {0} to {1}x{2}", warpplate.Name, num, num2), Color.Green);
				return;
			}
			args.Player.SendMessage("Something went wrong", Color.Red);
		}
		private static void setwarpplate(CommandArgs args)
		{
			if (args.Parameters.Count < 1)
			{
				args.Player.SendMessage("Invalid syntax! Proper syntax: /swp <warpplate name>", Color.Red);
				return;
			}
			if (WarpplatePlugin.Warpplates.InAreaWarpplateName(args.Player.TileX, args.Player.TileY) != null)
			{
				args.Player.SendMessage("There Is Already A Warpplate Located Here. Find A New Place", Color.Red);
				return;
			}
			string text = string.Join(" ", args.Parameters);
			int tx = (int)args.Player.X / 16 - 1;
			int ty = (int)args.Player.Y / 16;
			int width = 2;
			int height = 3;
			if (WarpplatePlugin.Warpplates.AddWarpplate(tx, ty, width, height, text, "", Main.worldID.ToString()))
			{
				args.Player.SendMessage("Warpplate Created: " + text, Color.Yellow);
				WarpplatePlugin.Warpplates.ReloadAllWarpplates();
				return;
			}
			args.Player.SendMessage("Warpplate Already Created: " + text + " already exists", Color.Red);
		}
		private static void delwarpplate(CommandArgs args)
		{
			if (args.Parameters.Count < 1)
			{
				args.Player.SendMessage("Invalid syntax! Proper syntax: /dwp <warpplate name>", Color.Red);
				return;
			}
			string text = string.Join(" ", args.Parameters);
			if (WarpplatePlugin.Warpplates.DeleteWarpplate(text))
			{
				args.Player.SendMessage("Deleted Warpplate: " + text, Color.Yellow);
				WarpplatePlugin.Warpplates.ReloadAllWarpplates();
				return;
			}
			args.Player.SendMessage("Could not find specified Warpplate", Color.Red);
		}
		private static void warpplatedest(CommandArgs args)
		{
			if (args.Parameters.Count < 2)
			{
				args.Player.SendMessage("Invalid syntax! Proper syntax: /swpd <Warpplate Name> <Name Of Destination Warpplate>", Color.Red);
				return;
			}
			if (WarpplatePlugin.Warpplates.adddestination(args.Parameters[0], args.Parameters[1]))
			{
				args.Player.SendMessage("Destination " + args.Parameters[1] + " Added To Warpplate " + args.Parameters[0], Color.Yellow);
				WarpplatePlugin.Warpplates.ReloadAllWarpplates();
				return;
			}
			args.Player.SendMessage("Could not find specified Warpplate or destination", Color.Red);
		}
		private static void removeplatedest(CommandArgs args)
		{
			if (args.Parameters.Count < 1)
			{
				args.Player.SendMessage("Invalid syntax! Proper syntax: /rwpd <Warpplate Name>", Color.Red);
				return;
			}
			if (WarpplatePlugin.Warpplates.removedestination(args.Parameters[0]))
			{
				args.Player.SendMessage("Removed Destination From Warpplate " + args.Parameters[0], Color.Yellow);
				WarpplatePlugin.Warpplates.ReloadAllWarpplates();
				return;
			}
			args.Player.SendMessage("Could not find specified Warpplate or destination", Color.Red);
		}
		private static string S(string s)
		{
			if (!string.IsNullOrEmpty(s))
			{
				return s;
			}
			return "(none)";
		}
		private static string D(Warpplate wp)
		{
			if (!string.IsNullOrEmpty(wp.Label))
			{
				return wp.Label;
			}
			return wp.Name + " (default)";
		}
		private static void wpi(CommandArgs args)
		{
			string name;
			if (args.Parameters.Count > 0)
			{
				name = string.Join(" ", args.Parameters);
			}
			else
			{
				name = WarpplatePlugin.Warpplates.InAreaWarpplateName(args.Player.TileX, args.Player.TileY);
			}
			Warpplate warpplate = WarpplatePlugin.Warpplates.FindWarpplate(name);
			if (warpplate == null)
			{
				args.Player.SendMessage("No Such Warpplate", Color.Red);
				return;
			}
			args.Player.SendMessage(string.Concat(new string[]
			{
				"Name: ",
				warpplate.Name,
				"; Label: ",
				WarpplatePlugin.D(warpplate),
				"Destination: ",
				WarpplatePlugin.S(warpplate.WarpDest),
				";"
			}), Color.HotPink);
			args.Player.SendMessage(string.Concat(new object[]
			{
				"X: ",
				warpplate.WarpplatePos.X,
				"; Y: ",
				warpplate.WarpplatePos.Y,
				"; W: ",
				warpplate.Area.Width,
				"; H: ",
				warpplate.Area.Height,
				"; Delay: ",
				warpplate.Delay - 1
			}), Color.HotPink);
		}
		private static void reloadwarp(CommandArgs args)
		{
			WarpplatePlugin.Warpplates.ReloadAllWarpplates();
		}
		private static void warpallow(CommandArgs args)
		{
			if (!WarpplatePlugin.Players[WarpplatePlugin.GetPlayerIndex(args.Player.Index)].warpplateuse)
			{
				args.Player.SendMessage("Warpplates Are Now Turned On For You");
			}
			if (WarpplatePlugin.Players[WarpplatePlugin.GetPlayerIndex(args.Player.Index)].warpplateuse)
			{
				args.Player.SendMessage("Warpplates Are Now Turned Off For You");
			}
			WarpplatePlugin.Players[WarpplatePlugin.GetPlayerIndex(args.Player.Index)].warpplateuse = !WarpplatePlugin.Players[WarpplatePlugin.GetPlayerIndex(args.Player.Index)].warpplateuse;
		}
		public static List<Group> FindGroup(string grp)
		{
			List<Group> list = new List<Group>();
			grp = grp.ToLower();
			foreach (Group current in TShock.Groups.groups)
			{
				if (current != null)
				{
					string text = current.Name.ToLower();
					if (text.Equals(grp))
					{
						return new List<Group>
						{
							current
						};
					}
					if (text.Contains(grp))
					{
						list.Add(current);
					}
				}
			}
			return list;
		}
	}
}
