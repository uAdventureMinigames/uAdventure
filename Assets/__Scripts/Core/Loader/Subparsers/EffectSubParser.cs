﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace uAdventure.Core
{
	[DOMParser("effect", "not-effect", "post-effect")]
	[DOMParser(typeof(Effects))]
	public class EffectSubParser : IDOMParser
    {

		public object DOMParse(XmlElement element, params object[] parameters)
        {
			var chapter = parameters [0] as Chapter;
			var effects = new Effects ();

            string tmpArgVal;
            int x = 0;
            int y = 0;
            string path = "";
            string id = "";
            bool animated = false, addeffect = true;
            List<AbstractEffect> effectlist;

			Effect currentEffect = null;

            foreach (XmlElement effect in element.ChildNodes)
            {
                addeffect = true;

                switch (effect.Name)
                {
                    case "cancel-action": currentEffect = new CancelActionEffect(); break;
                    case "activate":
                    case "deactivate":
                        tmpArgVal = effect.GetAttribute("flag");
                        if (!string.IsNullOrEmpty(tmpArgVal)) chapter.addFlag(tmpArgVal);

                        if (effect.Name == "activate") currentEffect = new ActivateEffect(tmpArgVal);
                        else                           currentEffect = new DeactivateEffect(tmpArgVal);
                        break;
                    case "set-value":
                    case "increment":
                    case "decrement":
						string var = effect.GetAttribute("var");
						int value = int.Parse(effect.GetAttribute("value") ?? "0");

                        if (effect.Name == "set-value")
                            currentEffect = new SetValueEffect(var, value);
                        else if (effect.Name == "increment")
                            currentEffect = new IncrementVarEffect(var, value);
                        else
                            currentEffect = new DecrementVarEffect(var, value);

                        chapter.addVar(var);
                        break;
                    case "macro-ref":
						currentEffect = new MacroReferenceEffect(effect.GetAttribute("id") ?? "");
                        break;
                    case "speak-char":
                        // Add the effect and clear the current string
						currentEffect = new SpeakCharEffect(effect.GetAttribute("idTarget"), effect.InnerText.ToString().Trim());
						((SpeakCharEffect)currentEffect).setAudioPath(effect.GetAttribute("uri") ?? "");
                        break;
                    case "trigger-last-scene":
                        currentEffect = new TriggerLastSceneEffect();
                        break;
                    case "play-sound":
                        // Store the path and background
                        bool background = true;
                        tmpArgVal = effect.GetAttribute("background");
						if (!string.IsNullOrEmpty(tmpArgVal))background = tmpArgVal.Equals("yes");

						path = effect.GetAttribute("uri") ?? "";

                        // Add the new play sound effect
                        currentEffect = new PlaySoundEffect(background, path);
                        break;

					case "consume-object": 		currentEffect = new ConsumeObjectEffect(effect.GetAttribute("idTarget") ?? ""); break;
					case "generate-object": 	currentEffect = new GenerateObjectEffect(effect.GetAttribute("idTarget") ?? ""); break;
					case "trigger-book": 		currentEffect = new TriggerBookEffect(effect.GetAttribute("idTarget") ?? ""); break;
					case "trigger-conversation": currentEffect = new TriggerConversationEffect(effect.GetAttribute("idTarget") ?? ""); break;
					case "trigger-cutscene": 	currentEffect = new TriggerCutsceneEffect(effect.GetAttribute("idTarget") ?? ""); break;
                    case "trigger-scene":
					
						x = int.Parse(effect.GetAttribute("x") ?? "0");
						y = int.Parse(effect.GetAttribute("y") ?? "0");
						string scene = effect.GetAttribute("idTarget") ?? "";
                        currentEffect = new TriggerSceneEffect(scene, x, y);
                        break;
					case "play-animation":
						x = int.Parse(effect.GetAttribute("x") ?? "0");
						y = int.Parse(effect.GetAttribute("y") ?? "0");
						path = effect.GetAttribute("uri") ?? "";
                        // Add the new play sound effect
                        currentEffect = new PlayAnimationEffect(path, x, y);
                        break;
					case "move-player":
						x = int.Parse(effect.GetAttribute("x") ?? "0");
						y = int.Parse(effect.GetAttribute("y") ?? "0");
                        // Add the new move player effect
                        currentEffect = new MovePlayerEffect(x, y);
                        break;
					case "move-npc":
						x = int.Parse(effect.GetAttribute("x") ?? "0");
						y = int.Parse(effect.GetAttribute("y") ?? "0");
						string npcTarget = effect.GetAttribute("idTarget") ?? "";
                        // Add the new move NPC effect
                        currentEffect = new MoveNPCEffect(npcTarget, x, y);
                        break;
					case "random-effect":
	                    // Add the new random effect
						var randomEffect = new RandomEffect (int.Parse (effect.GetAttribute ("probability") ?? "0"));

						Effects randomEffectList = DOMParserUtility.DOMParse<Effects> (effect, parameters);

                        randomEffect.setPositiveEffect(randomEffectList.getEffects()[0]);
                        randomEffect.setNegativeEffect(randomEffectList.getEffects()[1]);

                        currentEffect = randomEffect;
                        break;
                    case "wait-time":
                        // Add the new move NPC effect
						currentEffect = new WaitTimeEffect(int.Parse (effect.GetAttribute("time") ?? "0"));
                        break;
					case "show-text":
						x = int.Parse(effect.GetAttribute("x") ?? "0");
						y = int.Parse(effect.GetAttribute("y") ?? "0");
						var frontColor = effect.GetAttribute("frontColor") ?? "";
						var borderColor = effect.GetAttribute("borderColor") ?? "";

                        // Add the new ShowTextEffect
                        currentEffect = new ShowTextEffect(effect.InnerText.ToString().Trim(), x, y, frontColor, borderColor);
						((ShowTextEffect)currentEffect).setAudioPath( effect.GetAttribute("uri") ?? "");
                        break;
                    case "highlight-item":
						id = effect.GetAttribute("idTarget") ?? "";
						animated = "yes".Equals (effect.GetAttribute("animated"));

						int type = 0;
						tmpArgVal = effect.GetAttribute("type");
                        if (!string.IsNullOrEmpty(tmpArgVal))
                        {
                            if (tmpArgVal.Equals("none"))
                                type = HighlightItemEffect.NO_HIGHLIGHT;
                            if (tmpArgVal.Equals("green"))
                                type = HighlightItemEffect.HIGHLIGHT_GREEN;
                            if (tmpArgVal.Equals("red"))
                                type = HighlightItemEffect.HIGHLIGHT_RED;
                            if (tmpArgVal.Equals("blue"))
                                type = HighlightItemEffect.HIGHLIGHT_BLUE;
                            if (tmpArgVal.Equals("border"))
                                type = HighlightItemEffect.HIGHLIGHT_BORDER;
                        }
                        currentEffect = new HighlightItemEffect(id, type, animated);
                        break;
					case "move-object":

						x = int.Parse (effect.GetAttribute ("x") ?? "0");
						y = int.Parse (effect.GetAttribute ("y") ?? "0");

						id = effect.GetAttribute ("idTarget") ?? "";
						animated = "yes".Equals (effect.GetAttribute ("animated"));
						float scale = float.Parse (effect.GetAttribute ("scale") ?? "1.0", CultureInfo.InvariantCulture);
						int translateSpeed = int.Parse(effect.GetAttribute("translateSpeed") ?? "20");
						int scaleSpeed = int.Parse(effect.GetAttribute("scaleSpeed") ?? "20");

                        currentEffect = new MoveObjectEffect(id, x, y, scale, animated, translateSpeed, scaleSpeed);
                        break;
				case "speak-player":
                        // Add the effect and clear the current string
						currentEffect = new SpeakPlayerEffect (effect.InnerText.ToString ().Trim ());
						((SpeakPlayerEffect)currentEffect).setAudioPath (effect.GetAttribute ("uri") ?? "");
                        break;
                    case "condition":
                        addeffect = false;
						var currentConditions = DOMParserUtility.DOMParse (effect, parameters) as Conditions ?? new Conditions();
                        effectlist = effects.getEffects();
                        effectlist[effectlist.Count - 1].setConditions(currentConditions);
                        break;
                    case "documentation":
                        addeffect = false;
                        break;
				default:
						currentEffect = DOMParserUtility.DOMParse (effect) as AbstractEffect;
						addeffect = currentEffect != null;
						if(!addeffect) Debug.LogWarning("EFFECT NOT SUPPORTED: " + effect.Name);

                        break;
                }

                if (addeffect)
                    effects.add(currentEffect);
            }
        }
    }
}