using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryHierarchy : MonoBehaviour
{
    public struct OrderedStory
    {
        public int headerIdx;
        public int idx;
        public string nameEN;
        public string nameDE;
        // public string storyName;
        //public List<string> titles;
        

        public OrderedStory(int headerIdx, int idx, string nameEN, string nameDE)
        {
            this.headerIdx = headerIdx;
            this.idx = idx;
            // this.storyName = storyName;
            this.nameEN = nameEN;
            this.nameDE = nameDE;
        }
    }

    // private List<OrderedStory> orderedStories = new List<OrderedStory>();
    public Dictionary<string, OrderedStory> orderedStories = new Dictionary<string, OrderedStory>();

    // private List<string> _titles = new List<string>();
    // private List<string> titles = new List<string>();
    private OrderedStory os;


    // public List<OrderedStory> OrderedStories
    // {
    //     get{ return orderedStories;}
    // }

    public Dictionary<string, OrderedStory> OrderedStories
    {
        get{ return orderedStories;}
    }


    public void AddStoryToDictionary(string pi)
    {
        switch(pi)
        {
// Introduction
//      Motoko
            case"A_Onboarding":
                os = new OrderedStory(0,0,"Motoko","Motoko");
                orderedStories.Add(pi,os);
                break; 
//      Museum Choice
            case"A_StoryIntro_Part2":
                os = new OrderedStory(0,1,"Museum Choice","Wahl des Museums");
                orderedStories.Add(pi,os);
                break;

// Gemëldegalerie
//      The Merchant Georg Gisze
            case"gg_0":
                os = new OrderedStory(1,0,"Holbein","Holbein");
                orderedStories.Add(pi,os);
                break;
//      Impossible Bouquet
            case"gg_1":
                os = new OrderedStory(1,1,"Brueghel the Elder","Brueghel der Ältere");
                orderedStories.Add(pi,os);
                break;
//      Woman with a Pearl Necklace
            case"gg_2":
                os = new OrderedStory(1,2,"Vermeer van Delft","Vermeer von Delft");
                orderedStories.Add(pi,os);
                break;
//      Therbusch
            case"ArtJsonTest": /////////////////////////////////////////to change
                os = new OrderedStory(1,3,"Therbusch","Therbusch");
                orderedStories.Add(pi,os);
                break;

// Museum of Musical Instruments
//      The Mighty Wurlitzer
            case"arc4_wurlitzer_success":
                os = new OrderedStory(2,0,"The Mighty Wurlitzer","Der mächtige Wurlitzer");
                orderedStories.Add(pi,os);
                break;
//      Mixturtrautonium
            case"arc4_trautonium":
                os = new OrderedStory(2,1,"Mixtur-Trautonium","Mixtur-Trautonium");
                orderedStories.Add(pi,os);             
                break;
//      Cembalo
            case"arc4_cembalo":
                os = new OrderedStory(2,2,"Cembalo","Cembalo");
                orderedStories.Add(pi,os);       
                break; 

// Museum of Decorative Arts
//      Balenciaga
            case"kgm_0":
                os = new OrderedStory(3,0,"Balenciaga","Balenciaga");
                orderedStories.Add(pi,os);
                
                break;
//      Relic of St. George
            case"kgm_1":
                os = new OrderedStory(3,1,"Relic of St. George","Reliquie von St. Georg");
                orderedStories.Add(pi, os);               
                break; 
//      Memphis Regal
            case"kgm_2":
                os = new OrderedStory(3,2,"Memphis Regal","Memphis Regal");
                orderedStories.Add(pi,os);               
                break; 

// Outdoors
//      Arrival
            case"E_OUTSIDE_3": /////////////////////////////////////////to change
                os = new OrderedStory(4,0,"Arrival","Ankunft");
                orderedStories.Add(pi, os);
                break;
//      Tiergarten District
            case"E_OUTSIDE_6": /////////////////////////////////////////to change
                os = new OrderedStory(4,1,"Tiergarten District","Tiergartenviertel");
                orderedStories.Add(pi,os);
                break;
//      St. Matthew’s Church
            case"E_OUTSIDE_9": /////////////////////////////////////////to change
                os = new OrderedStory(4,2,"St. Matthew's Church","St. Matthäus-Kirche");
                orderedStories.Add(pi,os);
                break;
//      Neue Nationalgalerie
                os = new OrderedStory(4,3,"Neue Nationalgalerie","Neue Nationalgalerie");
                orderedStories.Add(pi,os);
                break;

            default:
                break;
        }
    }
}


//============================================================================

// A_MuseumChoice
// A_Onboarding
// A_storyIntro
// A_storyIntro_Part2
// A_WaitingToBeThere
// arc4_cembalo
// arc4_trautonium
// arc4_wurlitzer
// arc4_wurlitzer_success
// Arc_Interrupt_1
// Arc_Interrupt_2
// Arc_interrupt_3
// ArtJsonTest
// btTest
// E_OUTSIDE
// E_OUTSIDE_1
// E_OUTSIDE_2
// E_OUTSIDE_3
// E_OUTSIDE_4
// E_OUTSIDE_5
// E_OUTSIDE_6
// E_OUTSIDE_7
// E_OUTSIDE_8
// E_OUTSIDE_9
// E_OUTSIDE_10
// E_OUTSIDE_11
// E_OUTSIDE_12
// E_OUTSIDE_13
// gg_0
// gg_1
// gg_2
// In_Between_1
// In_Between_1_out
// In_Between_2
// In_Between_2_out
// In_Between_3
// In_Between_4
// KF_Entrance
// kgm_0
// kgm_1
// kgm_2
// Q_Testing
// Questionair
// short
// shortGG
// shortKGM
// shortMIM
// shortOut
// TestMap
// WaitForPlazaInbetween


