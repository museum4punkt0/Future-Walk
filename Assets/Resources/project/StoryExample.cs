﻿/*
------------------------------------------------
Generated by Cradle 0.0.0.0
https://github.com/daterre/Cradle

Original file: StoryExample.html
Story format: Harlowe
------------------------------------------------
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cradle;
using IStoryThread = System.Collections.Generic.IEnumerable<Cradle.StoryOutput>;
using Cradle.StoryFormats.Harlowe;

public partial class @StoryExample: Cradle.StoryFormats.Harlowe.HarloweStory
{
	#region Variables
	// ---------------

	public class VarDefs: RuntimeVars
	{
		public VarDefs()
		{
			VarDef("isenglish", () => this.@isenglish, val => this.@isenglish = val);
		}

		public StoryVar @isenglish;
	}

	public new VarDefs Vars
	{
		get { return (VarDefs) base.Vars; }
	}

	// ---------------
	#endregion

	#region Initialization
	// ---------------

	public readonly StoryMacros macros1;
	public readonly Cradle.StoryFormats.Harlowe.HarloweRuntimeMacros macros2;

	@StoryExample()
	{
		this.StartPassage = "Start";

		base.Vars = new VarDefs() { Story = this, StrictMode = true };

		macros1 = new StoryMacros() { Story = this };
		macros2 = new Cradle.StoryFormats.Harlowe.HarloweRuntimeMacros() { Story = this };

		base.Init();
		passage1_Init();
		passage2_Init();
		passage3_Init();
		passage4_Init();
		passage5_Init();
		passage6_Init();
		passage7_Init();
		passage8_Init();
		passage9_Init();
		passage10_Init();
		passage11_Init();
		passage12_Init();
		passage13_Init();
	}

	// ---------------
	#endregion

	// .............
	// #1: Start

	void passage1_Init()
	{
		this.Passages[@"Start"] = new StoryPassage(@"Start", new string[]{ "Bot", }, passage1_Main);
	}

	IStoryThread passage1_Main()
	{
		yield return lineBreak();
		yield return lineBreak();
		if(Vars.isenglish) {
			yield return lineBreak();
			yield return lineBreak();
			yield return text("On a bright sunny morning i woke up.");
			yield return lineBreak();
			yield return lineBreak();
			yield return text("\"My name is Bot\", i thought, shook my legs and begun to reassemble all my parts.");
			yield return lineBreak();
			yield return lineBreak();
			yield return link("Good morning.", "Good morning.", null);
			yield return lineBreak();
			yield return link("Why are you disassembled?", "disassembled?", null);
			yield return lineBreak();
			yield return lineBreak();
		}
		else {
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Es war ein schöner, sonniger Morgen als ich aufwachte.");
			yield return lineBreak();
			yield return lineBreak();
			yield return text("\"Ich heisse Bot\", dachte ich, schüttelte meine Beine und begann mich wieder zusammen zu setzen.");
			yield return lineBreak();
			yield return lineBreak();
			yield return link("Guten Morgen.", "Good morning.", null);
			yield return lineBreak();
			yield return link("Warum bist du zerlegt?", "disassembled?", null);
			yield return lineBreak();
			yield return lineBreak();
		}
		yield break;
	}


	// .............
	// #2: Good morning.

	void passage2_Init()
	{
		this.Passages[@"Good morning."] = new StoryPassage(@"Good morning.", new string[]{ "Bot", }, passage2_Main);
	}

	IStoryThread passage2_Main()
	{
		if(Vars.isenglish) {
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Good morning.");
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Can i show you an image?");
			yield return lineBreak();
			yield return lineBreak();
			yield return link("Sure, why not.", "showImage", null);
			yield return lineBreak();
			yield return lineBreak();
		}
		else {
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Guten Morgen.");
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Darf ich dir ein Bild zeigen?");
			yield return lineBreak();
			yield return lineBreak();
			yield return link("Klar, warum nicht.", "showImage", null);
			yield return lineBreak();
			yield return lineBreak();
		}
		yield break;
	}


	// .............
	// #3: disassembled?

	void passage3_Init()
	{
		this.Passages[@"disassembled?"] = new StoryPassage(@"disassembled?", new string[]{ "Bot", }, passage3_Main);
	}

	IStoryThread passage3_Main()
	{
		if(Vars.isenglish) {
			yield return lineBreak();
			yield return lineBreak();
			yield return text("I disassemble before going to sleep.");
			yield return lineBreak();
			yield return lineBreak();
			yield return text("It makes me feel more alive when i wake up.");
			yield return lineBreak();
			yield return lineBreak();
			yield return text("It is a fresh start into the day, everything feel clean and new.");
			yield return lineBreak();
			yield return lineBreak();
			yield return link("I see.", "showImage", null);
			yield return lineBreak();
			yield return lineBreak();
		}
		else {
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Ich zerlege mich immer vor dem schlafen gehen.");
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Es fühlt sich einfach gut an wenn ich aufwache.");
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Ein frischer Start in den neuen Tag, alles ist wie neu.");
			yield return lineBreak();
			yield return lineBreak();
			yield return link("Ach so.", "showImage", null);
			yield return lineBreak();
			yield return lineBreak();
		}
		yield break;
	}


	// .............
	// #4: showImage

	void passage4_Init()
	{
		this.Passages[@"showImage"] = new StoryPassage(@"showImage", new string[]{ "Bot", }, passage4_Main);
	}

	IStoryThread passage4_Main()
	{
		yield return htmlTag("<img src=\"artwork1\"/>");
		yield return lineBreak();
		yield return lineBreak();
		if(Vars.isenglish) {
			yield return lineBreak();
			yield return lineBreak();
			yield return link("Thanks for sharing this.", "Thanks for sharing this.", null);
			yield return lineBreak();
			yield return lineBreak();
		}
		else {
			yield return lineBreak();
			yield return lineBreak();
			yield return link("Danke fürs Teilen", "Thanks for sharing this.", null);
			yield return lineBreak();
			yield return lineBreak();
		}
		yield break;
	}


	// .............
	// #5: Thanks for sharing this.

	void passage5_Init()
	{
		this.Passages[@"Thanks for sharing this."] = new StoryPassage(@"Thanks for sharing this.", new string[]{  }, passage5_Main);
	}

	IStoryThread passage5_Main()
	{
		if(Vars.isenglish) {
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Hi my name is Andrea. I just come this way and see you guys chatting. ");
			yield return lineBreak();
			yield return lineBreak();
			yield return text("What's up?");
			yield return lineBreak();
			yield return lineBreak();
			yield return link("Hi Andrea.", "Hi Andrea.", null);
			yield return lineBreak();
			yield return link("Hello.", "Hello.", null);
			yield return lineBreak();
			yield return lineBreak();
		}
		else {
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Hi ich bin Andrea. Ich habe euch plaudern gesehen.");
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Um was geht es?");
			yield return lineBreak();
			yield return lineBreak();
			yield return link("Hi Andrea.", "Hi Andrea.", null);
			yield return lineBreak();
			yield return link("Hallo.", "Hello.", null);
			yield return lineBreak();
			yield return lineBreak();
		}
		yield break;
	}


	// .............
	// #6: Hi Andrea.

	void passage6_Init()
	{
		this.Passages[@"Hi Andrea."] = new StoryPassage(@"Hi Andrea.", new string[]{ "Bot", }, passage6_Main);
	}

	IStoryThread passage6_Main()
	{
		if(Vars.isenglish) {
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Hi Andrea.");
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Would you also like to see the Artwork i was showing just before?");
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Yes?");
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Here you go:");
			yield return lineBreak();
			yield return lineBreak();
		}
		else {
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Hi Andrea.");
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Möchtest du das Kunstwerk sehen das ich gerade gezeigt habe?");
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Ja?");
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Bitteschön:");
			yield return lineBreak();
			yield return lineBreak();
		}
		yield return lineBreak();
		yield return lineBreak();
		yield return htmlTag("<Artwork src=\"artwork1\"/>");
		yield return lineBreak();
		yield return lineBreak();
		macros1.delay(5000/1000f);
		yield return lineBreak();
		yield return lineBreak();
		yield return abort(goToPassage: "artreaction");
		yield return lineBreak();
		yield return link("artreaction", "artreaction", null);
		yield break;
	}


	// .............
	// #7: Hello.

	void passage7_Init()
	{
		this.Passages[@"Hello."] = new StoryPassage(@"Hello.", new string[]{  }, passage7_Main);
	}

	IStoryThread passage7_Main()
	{
		if(Vars.isenglish) {
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Would you like to join me for a little walk?");
			yield return lineBreak();
			yield return lineBreak();
			yield return text("This is where i plan going:");
			yield return lineBreak();
			yield return lineBreak();
			yield return htmlTag("<Map src=\"outside_01\"/>");
			yield return lineBreak();
			yield return lineBreak();
			yield return link("Ok. Let's go.", "walk", null);
			yield return lineBreak();
			yield return link("That looks very far.", "toofar", null);
			yield return lineBreak();
			yield return lineBreak();
		}
		else {
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Wollt ihr mit mir einen kleinen Spaziergang machen?");
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Hier wollte ich hin:");
			yield return lineBreak();
			yield return lineBreak();
			yield return htmlTag("<Map src=\"outside_01\"/>");
			yield return lineBreak();
			yield return lineBreak();
			yield return link("Ok. Lass uns gehen.", "walk", null);
			yield return lineBreak();
			yield return link("Das schaut weit aus.", "toofar", null);
			yield return lineBreak();
			yield return lineBreak();
		}
		yield break;
	}


	// .............
	// #8: walk

	void passage8_Init()
	{
		this.Passages[@"walk"] = new StoryPassage(@"walk", new string[]{ "Bot", }, passage8_Main);
	}

	IStoryThread passage8_Main()
	{
		if(Vars.isenglish) {
			yield return lineBreak();
			yield return lineBreak();
			yield return text("I would also join.");
			yield return lineBreak();
			yield return lineBreak();
		}
		else {
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Ich komme auch mit.");
			yield return lineBreak();
			yield return lineBreak();
		}
		yield return lineBreak();
		yield return lineBreak();
		yield return abort(goToPassage: "offwego");
		yield return lineBreak();
		yield return link("offwego", "offwego", null);
		yield break;
	}


	// .............
	// #9: toofar

	void passage9_Init()
	{
		this.Passages[@"toofar"] = new StoryPassage(@"toofar", new string[]{ "Bot", }, passage9_Main);
	}

	IStoryThread passage9_Main()
	{
		if(Vars.isenglish) {
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Yes, this looks really far.");
			yield return lineBreak();
			yield return lineBreak();
			yield return link("By the way. Can you say something?", "botvoice", null);
			yield return lineBreak();
			yield return lineBreak();
		}
		else {
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Ja, schaut weit aus.");
			yield return lineBreak();
			yield return lineBreak();
			yield return link("Übrigens. Kannst du sprechen?", "botvoice", null);
			yield return lineBreak();
			yield return lineBreak();
		}
		yield break;
	}


	// .............
	// #10: artreaction

	void passage10_Init()
	{
		this.Passages[@"artreaction"] = new StoryPassage(@"artreaction", new string[]{  }, passage10_Main);
	}

	IStoryThread passage10_Main()
	{
		if(Vars.isenglish) {
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Wow.");
			yield return lineBreak();
			yield return lineBreak();
			yield return link("Can you talk?", "botvoice", null);
			yield return lineBreak();
			yield return lineBreak();
		}
		else {
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Wow.");
			yield return lineBreak();
			yield return lineBreak();
			yield return link("Kannst du auch sprechen?", "botvoice", null);
			yield return lineBreak();
			yield return lineBreak();
		}
		yield break;
	}


	// .............
	// #11: offwego

	void passage11_Init()
	{
		this.Passages[@"offwego"] = new StoryPassage(@"offwego", new string[]{  }, passage11_Main);
	}

	IStoryThread passage11_Main()
	{
		if(Vars.isenglish) {
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Off we go.");
			yield return lineBreak();
			yield return lineBreak();
		}
		else {
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Los gehts.");
			yield return lineBreak();
			yield return lineBreak();
		}
		yield return lineBreak();
		yield return lineBreak();
		macros1.loadScene("StoryExampleWalking");
		yield break;
	}


	// .............
	// #12: Nice voice!

	void passage12_Init()
	{
		this.Passages[@"Nice voice!"] = new StoryPassage(@"Nice voice!", new string[]{  }, passage12_Main);
	}

	IStoryThread passage12_Main()
	{
		yield return text("yeah!");
		yield return lineBreak();
		yield return lineBreak();
		yield return abort(goToPassage: "Hello.");
		yield return lineBreak();
		yield return link("Hello.", "Hello.", null);
		yield break;
	}


	// .............
	// #13: botvoice

	void passage13_Init()
	{
		this.Passages[@"botvoice"] = new StoryPassage(@"botvoice", new string[]{ "Bot", }, passage13_Main);
	}

	IStoryThread passage13_Main()
	{
		if(Vars.isenglish) {
			yield return lineBreak();
			yield return lineBreak();
			yield return htmlTag("<a href=\"voice\">");
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Hello.");
			yield return lineBreak();
			yield return lineBreak();
			yield return text("This is how my voice sounds.");
			yield return lineBreak();
			yield return lineBreak();
			yield return htmlTag("</a>");
			yield return lineBreak();
			yield return lineBreak();
			yield return link("Nice voice!", "Nice voice!", null);
			yield return lineBreak();
			yield return lineBreak();
		}
		else {
			yield return lineBreak();
			yield return lineBreak();
			yield return htmlTag("<a href=\"voice\">");
			yield return lineBreak();
			yield return lineBreak();
			yield return text("Hallo.");
			yield return lineBreak();
			yield return lineBreak();
			yield return text("So klingt meine Stimme.");
			yield return lineBreak();
			yield return lineBreak();
			yield return htmlTag("</a>");
			yield return lineBreak();
			yield return lineBreak();
			yield return link("Schöne Stimme!", "Nice voice!", null);
			yield return lineBreak();
			yield return lineBreak();
		}
		yield break;
	}


}