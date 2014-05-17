using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Terraria;
using TShockAPI;
using TShockAPI.DB;
using MySql.Data.MySqlClient;
namespace PluginTemplate
{
	public class WarpplateManager
	{
		public List<Warpplate> Warpplates = new List<Warpplate>();
		private IDbConnection database;
		public WarpplateManager(IDbConnection db)
		{
			this.database = db;
			string arg_ED_0 = "Warpplates";
			SqlColumn[] array = new SqlColumn[11];
			array[0] = new SqlColumn("X1", MySqlDbType.Int32);
			array[1] = new SqlColumn("Y1", MySqlDbType.Int32);
			array[2] = new SqlColumn("width", MySqlDbType.Int32);
			array[3] = new SqlColumn("height", MySqlDbType.Int32);
			SqlColumn[] arg_7E_0 = array;
			int arg_7E_1 = 4;
			SqlColumn sqlColumn = new SqlColumn("WarpplateName", MySqlDbType.VarChar, new int?(50));
			sqlColumn.Primary = true;
			arg_7E_0[arg_7E_1] = sqlColumn;
			array[5] = new SqlColumn("WorldID", MySqlDbType.Text);
			array[6] = new SqlColumn("UserIds", MySqlDbType.Text);
			array[7] = new SqlColumn("Protected", MySqlDbType.Int32);
			array[8] = new SqlColumn("WarpplateDestination", MySqlDbType.VarChar, new int?(50));
			array[9] = new SqlColumn("Delay", MySqlDbType.Int32);
			array[10] = new SqlColumn("Label", MySqlDbType.Text);
			SqlTable sqlTable = new SqlTable(arg_ED_0, array);
			IQueryBuilder arg_10D_1;
			if (DbExt.GetSqlType(db) != SqlType.Sqlite)
			{
				IQueryBuilder queryBuilder = new MysqlQueryCreator();
				arg_10D_1 = queryBuilder;
			}
			else
			{
				arg_10D_1 = new SqliteQueryCreator();
			}
			SqlTableCreator sqlTableCreator = new SqlTableCreator(db, arg_10D_1);
			sqlTableCreator.EnsureExists(sqlTable);
			this.ReloadAllWarpplates();
		}
		public void ConvertDB()
		{
			try
			{
				DbExt.Query(this.database, "UPDATE Warpplates SET WorldID=@0, UserIds='', Delay=4", new object[]
				{
					Main.worldID.ToString()
				});
				this.ReloadAllWarpplates();
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
		}
		public void ReloadAllWarpplates()
		{
			try
			{
				using (QueryResult queryResult = DbExt.QueryReader(this.database, "SELECT * FROM Warpplates WHERE WorldID=@0", new object[]
				{
					Main.worldID.ToString()
				}))
				{
					this.Warpplates.Clear();
					while (queryResult.Read())
					{
						int num = queryResult.Get<int>("X1");
						int num2 = queryResult.Get<int>("Y1");
						int height = queryResult.Get<int>("height");
						int width = queryResult.Get<int>("width");
						int num3 = queryResult.Get<int>("Protected");
						string text = queryResult.Get<string>("UserIds");
						string name = queryResult.Get<string>("WarpplateName");
						string warpdest = queryResult.Get<string>("WarpplateDestination");
						int delay = queryResult.Get<int>("Delay");
						string label = queryResult.Get<string>("Label");
						string[] array = text.Split(new char[]
						{
							','
						}, StringSplitOptions.RemoveEmptyEntries);
						Warpplate warpplate = new Warpplate(new Vector2((float)num, (float)num2), new Rectangle(num, num2, width, height), name, warpdest, num3 != 0, Main.worldID.ToString(), label);
						warpplate.Delay = delay;
						try
						{
							for (int i = 0; i < array.Length; i++)
							{
								int item;
								if (int.TryParse(array[i], out item))
								{
									warpplate.AllowedIDs.Add(item);
								}
								else
								{
									Log.Warn("One of your UserIDs is not a usable integer: " + array[i]);
								}
							}
						}
						catch (Exception ex)
						{
							Log.Error("Your database contains invalid UserIDs (they should be ints).");
							Log.Error("A lot of things will fail because of this. You must manually delete and re-create the allowed field.");
							Log.Error(ex.ToString());
							Log.Error(ex.StackTrace);
						}
						this.Warpplates.Add(warpplate);
					}
				}
			}
			catch (Exception ex2)
			{
				Log.Error(ex2.ToString());
			}
		}
		public bool AddWarpplate(int tx, int ty, int width, int height, string Warpplatename, string Warpdest, string worldid)
		{
			if (this.GetWarpplateByName(Warpplatename) != null)
			{
				return false;
			}
			try
			{
				DbExt.Query(this.database, "INSERT INTO Warpplates (X1, Y1, width, height, WarpplateName, WorldID, UserIds, Protected, WarpplateDestination, Delay, Label) VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10);", new object[]
				{
					tx,
					ty,
					width,
					height,
					Warpplatename,
					worldid,
					"",
					1,
					Warpdest,
					4,
					""
				});
				this.Warpplates.Add(new Warpplate(new Vector2((float)tx, (float)ty), new Rectangle(tx, ty, width, height), Warpplatename, worldid, true, Warpdest, ""));
				return true;
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
			return false;
		}
		public bool DeleteWarpplate(string name)
		{
			Warpplate warpplateByName = this.GetWarpplateByName(name);
			if (warpplateByName != null)
			{
				int num = DbExt.Query(this.database, "DELETE FROM Warpplates WHERE WarpplateName=@0 AND WorldID=@1", new object[]
				{
					name,
					Main.worldID.ToString()
				});
				this.Warpplates.Remove(warpplateByName);
				if (num > 0)
				{
					return true;
				}
			}
			return false;
		}
		public bool SetWarpplateState(string name, bool state)
		{
			Warpplate warpplateByName = this.GetWarpplateByName(name);
			if (warpplateByName != null)
			{
				try
				{
					warpplateByName.DisableBuild = state;
					DbExt.Query(this.database, "UPDATE Warpplates SET Protected=@0 WHERE WarpplateName=@1 AND WorldID=@2", new object[]
					{
						state ? 1 : 0,
						name,
						Main.worldID.ToString()
					});
					return true;
				}
				catch (Exception ex)
				{
					Log.Error(ex.ToString());
				}
				return false;
			}
			return false;
		}
		public bool UpdateWarpplate(string name)
		{
			Warpplate warpplateByName = this.GetWarpplateByName(name);
			if (warpplateByName != null)
			{
				try
				{
					DbExt.Query(this.database, "UPDATE Warpplates SET width=@0, height=@1, Delay=@2, Label=@3 WHERE WarpplateName=@4 AND WorldID=@5", new object[]
					{
						warpplateByName.Area.Width,
						warpplateByName.Area.Height,
						warpplateByName.Delay,
						warpplateByName.Label,
						name,
						Main.worldID.ToString()
					});
					return true;
				}
				catch (Exception ex)
				{
					Log.Error(ex.ToString());
				}
				return false;
			}
			return false;
		}
		public Warpplate FindWarpplate(string name)
		{
			try
			{
				foreach (Warpplate current in this.Warpplates)
				{
					if (current.Name == name)
					{
						return current;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
			return null;
		}
		public bool InArea(int x, int y)
		{
			foreach (Warpplate current in this.Warpplates)
			{
				if (x >= current.Area.Left && x <= current.Area.Right && y >= current.Area.Top && y <= current.Area.Bottom && current.DisableBuild)
				{
					return true;
				}
			}
			return false;
		}
		public string InAreaWarpplateName(int x, int y)
		{
			foreach (Warpplate current in this.Warpplates)
			{
				if (x >= current.Area.Left && x <= current.Area.Right && y >= current.Area.Top && y <= current.Area.Bottom && current.DisableBuild)
				{
					return current.Name;
				}
			}
			return null;
		}
		public static List<string> ListIDs(string MergedIDs)
		{
			return MergedIDs.Split(new char[]
			{
				','
			}, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
		}
		public bool removedestination(string WarpplateName)
		{
			Warpplate warpplateByName = this.GetWarpplateByName(WarpplateName);
			if (warpplateByName != null)
			{
				int num = DbExt.Query(this.database, "UPDATE Warpplates SET WarpplateDestination=@0 WHERE WarpplateName=@1 AND WorldID=@2", new object[]
				{
					"",
					WarpplateName,
					Main.worldID.ToString()
				});
				warpplateByName.WarpDest = "";
				if (num > 0)
				{
					return true;
				}
			}
			return false;
		}
		public bool adddestination(string WarpplateName, string WarpDestination)
		{
			Warpplate warpplateByName = this.GetWarpplateByName(WarpplateName);
			if (warpplateByName != null)
			{
				int num = DbExt.Query(this.database, "UPDATE Warpplates SET WarpplateDestination=@0 WHERE WarpplateName=@1 AND WorldID=@2;", new object[]
				{
					WarpDestination,
					WarpplateName,
					Main.worldID.ToString()
				});
				warpplateByName.WarpDest = WarpDestination;
				if (num > 0)
				{
					return true;
				}
			}
			return false;
		}
		public List<Warpplate> ListAllWarpplates(string worldid)
		{
			List<Warpplate> list = new List<Warpplate>();
			try
			{
				foreach (Warpplate current in this.Warpplates)
				{
					list.Add(new Warpplate
					{
						Name = current.Name
					});
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
			return list;
		}
		public Warpplate GetWarpplateByName(string name)
		{
			return this.Warpplates.FirstOrDefault((Warpplate r) => r.Name.Equals(name) && r.WorldID == Main.worldID.ToString());
		}
		public string GetLabel(string name)
		{
			Warpplate warpplate = this.FindWarpplate(name);
			if (string.IsNullOrEmpty(warpplate.Label))
			{
				return name;
			}
			return warpplate.Label;
		}
	}
}
