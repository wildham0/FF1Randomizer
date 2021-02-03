﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using RomUtilities;

namespace FF1Lib
{
	public enum MusicShuffle
	{
		[Description("None")]
		None = 0,
		[Description("Standard")]
		Standard,
		[Description("Nonsensical")]
		Nonsensical,
		[Description("Disable Music")]
		MusicDisabled
	}

	public enum MenuColor
	{
		[Description("Default Blue")]
		Blue = 0x01,
		[Description("Dark Blue")]
		DarkBlue = 0x02,
		[Description("Purple")]
		Purple = 0x03,
		[Description("Pink")]
		Pink = 0x04,
		[Description("Red")]
		Red = 0x05,
		[Description("Orange")]
		Orange = 0x06,
		[Description("Dark Orange")]
		DarkOrange = 0x07,
		[Description("Brown")]
		Brown = 0x08,
		[Description("Light Green")]
		LightGreen = 0x09,
		[Description("Green")]
		Green = 0x0A,
		[Description("Dark Green")]
		DarkGreen = 0x0B,
		[Description("Cyan")]
		Cyan = 0x0C,
		[Description("Black")]
		Black = 0x0F,
	}

	public enum MapmanSlot
	{
		[Description("Leader")]
		Leader = 0x00,
		[Description("Second")]
		Second = 0x01,
		[Description("Third")]
		Third = 0x02,
		[Description("Fourth")]
		Fourth = 0x03,
	}

	public enum Fate
	{
		[Description("Spare")]
		Spare = 0,
		[Description("Kill")]
		Kill = 2,
	}
	public partial class FF1Rom
	{
		public const int TyroPaletteOffset = 0x30FC5;
		public const int TyroSpriteOffset = 0x20560;

		public const int PaletteOffset = 0x30F20;
		public const int PaletteSize = 4;
		public const int PaletteCount = 64;

		public void FunEnemyNames(bool teamSteak)
		{
			var enemyText = ReadText(EnemyTextPointerOffset, EnemyTextPointerBase, EnemyCount);

			enemyText[1] = "GrUMP";
			enemyText[2] = "RURURU"; // +2
			enemyText[3] = "GrrrWOLF"; // +2
			enemyText[28] = "GeORGE";
			enemyText[30] = "R.SNEK"; // +3
			enemyText[31] = "GrSNEK"; // +1
			enemyText[32] = "SeaSNEK"; // -1
			enemyText[40] = "iMAGE";
			enemyText[56] = "EXPEDE"; // +2
			enemyText[66] = "White D";
			enemyText[72] = "MtlSLIME"; // +3
			if (teamSteak)
			{
				enemyText[85] = "STEAK"; // +1
				enemyText[86] = "T.BONE"; // +1
			}
			enemyText[92] = "NACHO"; // -1
			enemyText[106] = "Green D"; // +2
			enemyText[111] = "OKAYMAN"; // +1

			// Moving IMP and GrIMP gives me another 10 bytes, for a total of 19 extra bytes, of which I'm using 16.
			var enemyTextPart1 = enemyText.Take(2).ToArray();
			var enemyTextPart2 = enemyText.Skip(2).ToArray();
			WriteText(enemyTextPart1, EnemyTextPointerOffset, EnemyTextPointerBase, 0x2CFEC);
			WriteText(enemyTextPart2, EnemyTextPointerOffset + 4, EnemyTextPointerBase, EnemyTextOffset);
		}

		public void PaletteSwap(MT19337 rng)
		{
			var palettes = Get(PaletteOffset, PaletteSize * PaletteCount).Chunk(PaletteSize);

			palettes.Shuffle(rng);

			Put(PaletteOffset, Blob.Concat(palettes));
		}

		public void TeamSteak()
		{
			Put(TyroPaletteOffset, Blob.FromHex("302505"));
			Put(TyroSpriteOffset, Blob.FromHex(
				"00000000000000000000000000000000" + "00000000000103060000000000000001" + "001f3f60cf9f3f7f0000001f3f7fffff" + "0080c07f7f87c7e60000008080f8f8f9" + "00000080c0e0f0780000000000000080" + "00000000000000000000000000000000" +
				"00000000000000000000000000000000" + "0c1933676f6f6f6f03070f1f1f1f1f1f" + "ffffffffffffffffffffffffffffffff" + "e6e6f6fbfdfffffff9f9f9fcfefefefe" + "3c9e4e26b6b6b6b6c0e0f0f878787878" + "00000000000000000000000000000000" +
				"00000000000000000000000000000000" + "6f6f6f6f673b190f1f1f1f1f1f070701" + "fffffec080f9fbffffffffffff8787ff" + "ff3f1f1f3ffdf9f3fefefefefefefefc" + "b6b6b6b6b6b6b6b67878787878787878" + "00000000000000000000000000000000" +
				"00000000000000000000000000000000" + "07070706060707070100000101010101" + "ffffff793080c0f0fffc3086cfffffff" + "e7fefcf9f26469e3f80103070f9f9e1c" + "264c983060c08000f8f0e0c080000000" + "00000000000000000000000000000000" +
				"00000000000000000000000000000000" + "07070706060301010101010101000000" + "f9f9f9797366ece8fefefefefcf97377" + "c68c98981830606038706060e0c08080" + "00000000000000000000000000000000" + "00000000000000000000000000000000" +
				"00000000000000000000000000000000" + "01010101010000000000000000000000" + "fb9b9b9b98ff7f006767676767000000" + "6060606060c080008080808080000000" + "00000000000000000000000000000000" + "00000000000000000000000000000000"));
		}

		public void DisableSpellCastScreenFlash()
		{
			//just load the original battleground background in place of the flash color, it will still use the same number of frames
			Put(0x32051, Blob.FromHex("AD446D"));
		}

		public void ShuffleLeader(MT19337 rng)
		{
			byte leader = (byte)(rng.Between(0, 3) << 6);
			Data[0x7D8BC] = leader;
			Data[0x7E933] = leader;
		}

		public void ShuffleMusic(MusicShuffle mode, MT19337 rng)
		{
			switch (mode)
			{
				case MusicShuffle.Standard:
					List<byte> overworldTracks = new List<byte> { 0x41, 0x42, 0x44, 0x45, 0x46, 0x47, 0x4A, 0x4F };
					List<byte> townTracks = new List<byte> { 0x41, 0x42, 0x45, 0x46, 0x47, 0x48, 0x4A, 0x4F, 0x51 };
					List<byte> dungeonTracks = new List<byte> { 0x41, 0x42, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x52, 0x53 };

					overworldTracks.Shuffle(rng);
					townTracks.Shuffle(rng);
					dungeonTracks.Shuffle(rng);

					//Overworld
					Data[0x7C649] = overworldTracks[0];
					Data[0x7C6F9] = overworldTracks[0];
					Data[0x7C75A] = overworldTracks[0];
					Data[0x7C75B] = overworldTracks[0];

					//Ship
					Data[0x7C62D] = overworldTracks[1];
					Data[0x7C75D] = overworldTracks[1];

					//Airship
					Data[0x7C235] = overworldTracks[2];
					Data[0x7C761] = overworldTracks[2];

					//Remove used songs from other pools
					var usedTracks = overworldTracks.Take(3).ToList();
					townTracks = townTracks.Except(usedTracks).ToList();

					//Town
					Data[0x7CFC3] = townTracks[0];

					//Castle
					Data[0x7CFC4] = townTracks[1];

					//Shop
					Data[0x3A351] = townTracks[2];
					Data[0x3A56E] = townTracks[2];
					Data[0x3A597] = townTracks[2];

					//Menu
					Data[0x3ADB4] = townTracks[3];
					Data[0x3B677] = townTracks[3];
					Data[0x3997F] = townTracks[3]; //Lineup menu

					//Remove used songs from other pools
					usedTracks.AddRange(townTracks.Take(4));
					dungeonTracks = dungeonTracks.Except(usedTracks).ToList();

					//Dungeons
					Data[0x7CFC5] = dungeonTracks[0];
					Data[0x7CFC6] = dungeonTracks[1];
					Data[0x7CFC7] = dungeonTracks[2];
					Data[0x7CFC8] = dungeonTracks[3];
					Data[0x7CFC9] = dungeonTracks[4];
					Data[0x7CFCA] = dungeonTracks[5];

					break;

				case MusicShuffle.Nonsensical: //They asked for it...
					List<byte> tracks = new List<byte> { 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F, 0x51, 0x52, 0x53, 0x55 };
					tracks.Shuffle(rng);

					//Overworld
					Data[0x7C649] = tracks[0];
					Data[0x7C6F9] = tracks[0];
					Data[0x7C75A] = tracks[0];
					Data[0x7C75B] = tracks[0];

					//Ship
					Data[0x7C62D] = tracks[1];
					Data[0x7C75D] = tracks[1];

					//Airship
					Data[0x7C235] = tracks[2];
					Data[0x7C761] = tracks[2];

					//Tilesets 1-8
					Data[0x7CFC3] = tracks[3]; //Town
					Data[0x7CFC4] = tracks[4]; //Castle
					Data[0x7CFC5] = tracks[5];
					Data[0x7CFC6] = tracks[6];
					Data[0x7CFC7] = tracks[7];
					Data[0x7CFC8] = tracks[8];
					Data[0x7CFC9] = tracks[9];
					Data[0x7CFCA] = tracks[10];

					//Title
					Data[0x3A226] = tracks[11];

					//Shop
					Data[0x3A351] = tracks[12];
					Data[0x3A56E] = tracks[12];
					Data[0x3A597] = tracks[12];

					//Menu
					Data[0x3ADB4] = tracks[13];
					Data[0x3B677] = tracks[13];
					Data[0x3997F] = tracks[13]; //Lineup menu

					//Ending
					Data[0x37804] = tracks[14];

					//Bridge Cutscene
					Data[0x3784E] = tracks[15];

					//Battle Fanfare
					Data[0x31E44] = tracks[16];

					//Gameover
					Data[0x3C5EF] = tracks[17];

					//Battle
					//Data[0x2D9C1] = Songs[rng.Between(0, Songs.Count - 1)];

					//Mini Things
					Data[0x36E86] = tracks[rng.Between(0, tracks.Count - 1)]; //minigame
					Data[0x27C0D] = tracks[rng.Between(0, tracks.Count - 1)]; //minimap

					break;

				case MusicShuffle.MusicDisabled:
					//Set Sq1, Sq2, and Tri channels for crystal theme all point to the same music data
					Put(0x34000, Blob.FromHex("C080C080C080"));
					//Overwrite beginning of crystal theme with a song that initializes properly but plays no notes
					Put(0x340C0, Blob.FromHex("FDF805E0D8C7D0C480"));

					List<int> AllSongs = new List<int>
					{
						0x7C649, 0x7C6F9, 0x7C75A, 0x7C75B, 0x7C62D, 0x7C75D,
						0x7C235, 0x7C761, 0x7CFC3, 0x7CFC4, 0x7CFC5, 0x7CFC6,
						0x7CFC7, 0x7CFC8, 0x7CFC9, 0x7CFCA, 0x3A226, 0x3A351,
						0x3A56E, 0x3A597, 0x3ADB4, 0x3B677, 0x3997F, 0x37804,
						0x3784E, 0x31E44, 0x3C5EF, 0x2D9C1, 0x36E86, 0x27C0D
					};
					//Set all music playback calls to play the new empty song
					foreach (int address in AllSongs)
					{
						Data[address] = 0x41;
					}

					break;
			}
		}

		public void DynamicWindowColor(MenuColor menuColor)
		{
			// This is an overhaul of LoadBorderPalette_Blue that enhances it to JSR to
			// DrawMapPalette first. That allows us to wrap that with a dynamic load of
			// the bg color after it sets it to the default one.
			/*
				LoadBorderPalette_Dynamic:
				JSR $D862 ; JSR to DrawMapPalette
				LDY $60FB ; Load dynamic palette color to Y

				LoadBorderPalette_Y:
				LDA $60FC ; Back up current bank
				PHA
				LDA #$0F
				JSR $FE03 ; SwapPRG_L
				JSR $8700 ; Jump out to palette writing code. Dynamic Color in Y
				PLA
				JSR $FE03 ; SwapPRG_L
				RTS
			*/
			Put(0x7EB29, Blob.FromHex("2062D8ACFB60ADFC6048A90F2003FE200087682003FE60"));

			// The battle call site needs black not the dynamic color so we jump right to
			// the operation after that when calling from battle init.
			Put(0x7EB90, Blob.FromHex("A00F4C2FEB"));

			// Modify two calls to DrawMapPalette to call our LoadBorderPalette_Dynamic which
			// starts with a JSR to DrawMapPalette and then adds the dynamic menu color.
			Put(0x7CF8F, Blob.FromHex("29EB"));
			Put(0x7CF1C, Blob.FromHex("29EB"));

			// Modify Existing calls to LoadBorderPalette_Blue up three bytes to where it starts
			Put(0x7EAB7, Blob.FromHex("2CEB"));
			Put(0x7EB58, Blob.FromHex("2CEB"));

			// There are two unfinished bugs in the equipment menu that use palettes 1 and 2
			// for no reason and need to use 3 now. They are all mirrors in vanilla.
			Put(0x3BE53, Blob.FromHex("EAEAEAEAEAEAEAEAEAEA"));
			Data[0x3BEF7] = 0x60;

			// Finally we need to also make the lit orb palette dynamic so lit orbs bg matches.
			// I copy the original code and add LDA/STA at the end for the bg color, and put
			// it over some unused garbage at the bottom of Bank E @ [$BF3A :: 0x3BF4A]
			/*
				LDX #$0B ; Straight copy from EnterMainMenu
				Loop:
				  LDA $AD78, X
				  STA $03C0, X
				  DEX
				  BPL Loop

				LDA $60FB ; Newly added to load and set dynamic palette color for lit orb
				STA $03C2
				RTS
			*/
			Put(0x3BF3A, Blob.FromHex("A20BBD78AD9DC003CA10F7ADFB608DC2038DC60360"));
			Put(0x3ADC2, Blob.FromHex("203ABFEAEAEAEAEAEAEAEA"));

			// Dynamic location initial value
			Data[0x30FB] = (byte)menuColor;

			// Hardcoded spot for opening "cinematic"
			Data[0x03A11C] = (byte)menuColor;
			Data[0x03A2D3] = (byte)menuColor;
		}

		public void EnableModernBattlefield()
		{
			// Since we're changing the window color in the battle scene we need to ensure that
			// $FF tile remains opaque like in the menu screen. That battle init code
			// overwrites it with transparent so we skip that code here. Since this is fast
			// enough we end up saving a frame we add a wait for VBlank to take the same total time.
			Put(0x7F369, Blob.FromHex("2000FEEAEAEAEAEAEAEAEAEAEAEAEAEAEA"));
			Put(0x7EB90, Blob.FromHex("4C29EB"));

			// Don't draw big battle boxes around the enemies, party, and individual stats.
			// Instead draw one box in the lower right corner around the new player stats.
			Put(0x7F2E4, Blob.FromHex("A9198538A913A206A00A20E2F3EAEAEAEAEAEAEAEAEAEAEAEAEA"));
			Put(0x7F2FB, Blob.FromHex("EAEAEAEAEAEAEAEAEAEAEAEAEAEAEAEAEAEAEAEAEAEAEAEAEA"));

			// The bottom row of these boxes was occluded by the Command Menu and enemy list so
			// there is code to redraw it whenever it would be exposed that we early return to skip.
			Data[0x7F62D] = 0x60;

			// To match later games and make better use of screen real estate we move all the bottom
			// boxes down a tile. This requires rewriting most of the battle box and text positioning
			// LUTs. They are largely formatted in a HeaderByte, X, Y, W, H system.
			// lut_EnemyRosterBox      HDXXYYWWHH
			Put(0x7F9E4, Blob.FromHex("0001010B0A"));

			//                           BOX      TEXT
			// lut_CombatBoxes:        HDXXYYWWHHHDXXYY
			Put(0x7F9E9, Blob.FromHex("0001010A04010202" + // attacker name
									  "000B010C04010C02" + // their attack("FROST", "2Hits!" etc)
									  "0001040A04010205" + // defender name
									  "000B040B04010C05" + // damage
									  "0001071804010208"));// bottom message("Terminated", "Critical Hit", etc)

			// lut_BattleCommandBox    HDXXYYWWHH
			Put(0x7FA1B, Blob.FromHex("000C010D0A"));

			// lut for Command Text    HDXXYYTPTR
			Put(0x7FA20, Blob.FromHex("010E0239FA" + // FIGHT
									  "010E043EFA" + // MAGIC
									  "010E0643FA" + // DRINK
									  "010E0848FA" + // ITEM
									  "0114024DFA"));// RUN

			// Edit lut for where to draw character status
			Put(0x32B21, Blob.FromHex("9A22DA221A235A23"));

			// Move character sprites to the right edge and space them a little apart
			byte xCoord = 0xD8;
			Data[0x31741] = xCoord;
			Data[0x3197D] = xCoord;                // overwrite hardcoded when stoned
			Data[0x31952] = (byte)(xCoord - 0x08); // 8px to the left when dead.

			byte yCoord = 0x2B;
			for (int i = 0; i < 4; ++i)
			{
				Data[0x3174F + (i * 5)] = (byte)(yCoord + (i * 28));
			}

			// Update Several 16 Byte LUTS related to cursors
			for (int i = 0; i < 8; ++i)
			{
				Data[0x31F85 + i * 2] = (byte)(xCoord - 0x10);            // X Coord of Character targeting cursor
				Data[0x31F86 + i * 2] = (byte)(yCoord + 4 + ((i % 4) * 0x1C));  // Y Coord of Character targeting cursor
				Data[0x31F76 + i * 2] = (byte)(0xA7 + (Math.Min(i, 4) % 4 * 0x10)); // Y Coord of Command Menu cursor (last four are identical)
			}

			// Start backdrop rows two tiles left and one upward. This matches FF2/3
			Data[0x7F34B] = 0x20;
			Data[0x7F352] = 0x40;
			Data[0x7F359] = 0x60;
			Data[0x7F360] = 0x80;

			// Adjust backdrop to draw in sets of 8 tiles so it appears to keep repeating.
			Put(0x7F38C, Blob.FromHex("A008849B20A0F320A0F320A0F3EAEAEAEA"));

			// Shorten the DrawStatus section to print only name or status, not both. Just one line.
			/* ASM Snippet
				LDY #$01        ; Need a one offset into
				LDA ($82), Y    ; btl_ob_charstat_ptr + 1 is status
				LSR             ; Shift dead bit to carry
				BCS skip        ; If dead print name
				BEQ skip        ; If healthy print name
				LDY #$09        ; otherwise load 9 to print status string
				skip:
				JSR $AAFC       ; JSR DrawStatusRow
			*/
			Put(0x32AB0, Blob.FromHex("A001B1824AB004F002A00920FCAAEAEAEAEAEAEA"));

			// Overwrite the upper portion of the default attribute table to all bg palette
			Put(0x7F400, Blob.FromHex("0000000000000000"));
			Put(0x7F408, Blob.FromHex("0000000000000000"));

			// Fix NT bits inside the drawing sequence of mixed enemies for expanded backdrop.
			// Before it would reset the palette to greyscale because it used to be borders.
			Data[0x2E6C9] = 0x70;
			Data[0x2E6CD] = 0xB0;

			// Fix NT bits inside chaostsa.bin and fiendtsa.bin
			Data[0x2D4C8] = 0xB0;
			for (int i = 0; i < 0x0140; i += 0x50)
			{
				Data[0x2D320 + i] = 0x00;
			}
		}

		public void UseVariablePaletteForCursorAndStone()
		{
			// The masamune uses the same palette as the cursor and stone characters
			// so we can free up a whole palette if we reset the varies palette to
			// the masmune palette after every swing and magic annimation. The only
			// drawback is that stoned characters will flash with attacks and magic.

			// Change UpdateVariablePalette to edit Palette 3
			Data[0x32B35] = 0xA4;
			Data[0x32B3B] = 0xA5;
			Data[0x32B41] = 0xA6;
			Data[0x32B46] = 0xA3;
			Data[0x32B4E] = 0xA6;

			// Make magic use palette 3
			Data[0x318F0] = 0x03;

			// Make sparks use palette 3
			Data[0x33E47] = 0x03;

			// Weapon palettes are embedded in this lut with their coordinates.
			Put(0x3202C, Blob.FromHex("0001020303030303"));
			Put(0x32034, Blob.FromHex("0100030243434343"));

			// Increase loop variable to do 12 colors when fading sprites in and out for inn animation.
			Data[0x7FF23] = 12;
			Data[0x7FF43] = 12;
			Data[0x7FF65] = 12;

			// Enable this feature by rewriting the JSR BattleFrame inside UpdateSprites_BattleFrame
			Put(0x31904, Blob.FromHex("20F1FD207CA060"));
		}

		public void DisableDamageTileFlicker()
		{
			Data[0x7C7E2] = 0xA9;
		}

		public void ChangeLute(MT19337 rng)
		{
			var newInstruments = new List<string> {"BASS", "LYRE", "HARP", "VIOLA", "CELLO", "PIANO", "ORGAN", "FLUTE", "OBOE", "PICCOLO", "FLUTE", "WHISTLE", "HORN", "TRUMPET",
				"BAGPIPE", "DRUM", "VIOLIN", "DBLBASS", "GUITAR", "BANJO", "FIDDLE", "MNDOLIN", "CLARNET", "BASSOON", "TROMBON", "TUBA", "BUGLE", "MARIMBA", "XYLOPHN","SNARE D",
				"BASS D", "TMBRINE", "CYMBALS", "TRIANGL", "COWBELL", "GONG", "TRUMPET", "SAX", "TIMPANI", "B GRAND", "HURDY G", "FLUGEL", "SONG", "KAZOO", "FOGHORN", "AIRHORN",
				"VUVUZLA", "OCARINA", "PANFLUT", "SITAR", "HRMNICA", "UKULELE", "THREMIN", "DITTY", "JINGLE", "LIMRICK", "POEM", "HAIKU", "OCTBASS", "HRPSCRD", "FLUBA", "AEOLUS",
				"TESLA", "STLDRUM", "DGDRIDO", "WNDCHIM" };

			var itemnames = ReadText(ItemTextPointerOffset, ItemTextPointerBase, ItemTextPointerCount);
			var dialogs = ReadText(dialogsPointerOffset, dialogsPointerBase, dialogsPointerCount);

			var newLute = newInstruments.PickRandom(rng);
			var dialogsUpdate = new Dictionary<int, string>();
			var princessDialogue = dialogs[0x06].Split(new string[] { "LUTE" }, System.StringSplitOptions.RemoveEmptyEntries);
			var monkDialogue = dialogs[0x35].Split(new string[] { "LUTE" }, System.StringSplitOptions.RemoveEmptyEntries);
			
			if (princessDialogue.Length > 1)
				dialogsUpdate.Add(0x06, princessDialogue[0] + newLute + princessDialogue[1]);

			if (monkDialogue.Length > 1)
				dialogsUpdate.Add(0x35, monkDialogue[0] + newLute + monkDialogue[1].Substring(0,14) + "\n" + monkDialogue[1].Substring(15, 10).Replace('\n',' '));

			// Add extra dialogues that might contain the LUTE if the NPChints flag is enabled or if Astos Shuffle is enabled
			var otherNPCs = new List<byte> {
				0x45, 0x53, 0x69, 0x82, 0x8C, 0xAA, 0xCB, 0xDC, 0x9D, 0x70, 0xE3, 0xE1, 0xB6, // NPChints
				0x02, 0x0E, 0x12, 0x14, 0x16, 0x19, 0x1E, 0xCD, 0x27, 0x23, 0x2B // SuffleAstos
			};

			for (int i = 0; i < otherNPCs.Count(); i++)
			{
				var tempDialogue = dialogs[otherNPCs[i]].Split(new string[] { "LUTE" }, System.StringSplitOptions.RemoveEmptyEntries);
				if (tempDialogue.Length > 1)
					dialogsUpdate.Add(otherNPCs[i], tempDialogue[0] + newLute + tempDialogue[1]);
			}

			if (dialogsUpdate.Count > 0)
				InsertDialogs(dialogsUpdate);

			itemnames[(int)Item.Lute] = newLute;
			WriteText(itemnames, ItemTextPointerOffset, ItemTextPointerBase, ItemTextOffset);
		}

		public void HurrayDwarfFate(Fate fate, NPCdata npcdata, MT19337 rng)
		{
			if (fate == Fate.Spare)
			{
				// Protect Hurray Dwarf from NPC guillotine
				npcdata.SetRoutine(ObjectId.DwarfcaveDwarfHurray, newTalkRoutines.Talk_norm);
			}
			else
			{
				// Whether NPC guillotine is on or not, kill Hurray Dwarf
				npcdata.SetRoutine(ObjectId.DwarfcaveDwarfHurray, newTalkRoutines.Talk_kill);

				// Change the dialogue
				var dialogueStrings = new List<string>
				{
				    "No! I'm gonna disappear.\nYou'll never see\nme again. Please,\nI don't want to die.",
					"If you strike me down,\nI shall become more\npowerful than you can\npossibly imagine.",
					"Freeeeedom!!",
					"I've seen things you\npeople wouldn't believe.\nAll those moments will\nbe lost in time..\nlike tears in rain..\nTime to die.",
					"Become vengeance, David.\nBecome wrath.",
					"My only regret..\nis that I have boneitis.",
					"No, not the bees!\nNOT THE BEES!\nAAAAAAAAGH!\nTHEY'RE IN MY EYES!\nMY EYES! AAAAAAAAAAGH!",
					"This is blasphemy!\nThis is madness!",
					"Not like this..\nnot like this..",
					"Suicide squad, attack!\n\n\n\nThat showed 'em, huh?",
					"Well, what are you\nwaiting for?\nDo it. DO IT!!",
					"The path you walk on has\nno end. Each step you\ntake is paved with the\ncorpses of your enemies.\nTheir souls will haunt\nyou forever. Hear me!\nMy spirit will be\nwatching you!",
					"K-Kefka..!\nY-you're insane.."
				};

				//Put new dialogue to E6 since another Dwarf also says hurray
				InsertDialogs(0xE6, dialogueStrings.PickRandom(rng));
				npcdata.GetTalkArray(ObjectId.DwarfcaveDwarfHurray)[(int)TalkArrayPos.dialogue_1] = 0xE6;
				npcdata.GetTalkArray(ObjectId.DwarfcaveDwarfHurray)[(int)TalkArrayPos.dialogue_2] = 0xE6;
				npcdata.GetTalkArray(ObjectId.DwarfcaveDwarfHurray)[(int)TalkArrayPos.dialogue_3] = 0xE6;
			}
		}

	}

}
