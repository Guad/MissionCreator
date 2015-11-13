using System;
using System.Collections.Generic;

namespace MissionCreator.StaticData
{
    public static class AnimData
    {
        public static Dictionary<string, Tuple<string, string, string>[]> Database = new Dictionary<string, Tuple<string, string, string>[]>
        {
            {"Hand Animations", new []
            {
               new Tuple<string, string, string>("Hands Up", "random@getawaydriver", "idle_a"),
               new Tuple<string, string, string>("Scared Hands Up", "missfbi5ig_20b", "hands_up_scientist"),
            }},
            {"Gestures Female", new []
            {
                new Tuple<string, string, string>("Yes", "random@getawaydriver", "gesture_nod_yes_hard"),
                new Tuple<string, string, string>("Bring it on", "gestures@f@standing@casual", "gesture_bring_it_on"),
                new Tuple<string, string, string>("Bye", "gestures@f@standing@casual", "gesture_bye_hard"),
                new Tuple<string, string, string>("Come Here", "gestures@f@standing@casual", "gesture_come_here_hard"),
                new Tuple<string, string, string>("Damn!", "gestures@f@standing@casual", "gesture_damn"),
                new Tuple<string, string, string>("Displeased", "gestures@f@standing@casual", "gesture_displeased"),
                new Tuple<string, string, string>("Easy now", "gestures@f@standing@casual", "gesture_easy_now"),
                new Tuple<string, string, string>("Hello", "gestures@f@standing@casual", "gesture_hello"),
                new Tuple<string, string, string>("I Will", "gestures@f@standing@casual", "gesture_i_will"),
                new Tuple<string, string, string>("Me", "gestures@f@standing@casual", "gesture_me_hard"),
                new Tuple<string, string, string>("No way!", "gestures@f@standing@casual", "gesture_no_way"),
                new Tuple<string, string, string>("Point", "gestures@f@standing@casual", "gesture_point"),
                new Tuple<string, string, string>("Shrug", "gestures@f@standing@casual", "gesture_shrug_hard"),
                new Tuple<string, string, string>("What?", "gestures@f@standing@casual", "gesture_what_hard"),
                new Tuple<string, string, string>("Why?", "gestures@f@standing@casual", "gesture_why"),
                new Tuple<string, string, string>("You", "gestures@f@standing@casual", "gesture_you_hard"),
                new Tuple<string, string, string>("It's mine!", "gestures@f@standing@casual", "getsure_its_mine"),
                new Tuple<string, string, string>("Shrug", "gestures@f@standing@casual", "gesture_shrug_hard"),
                new Tuple<string, string, string>("Shrug", "gestures@f@standing@casual", "gesture_shrug_hard"),
                new Tuple<string, string, string>("Shrug", "gestures@f@standing@casual", "gesture_shrug_hard"),
                new Tuple<string, string, string>("Nuh-uh", "mini@prostitutestalk", "street_argue_f_a"),
            }},
            {"Gestures Male", new []
            {
                new Tuple<string, string, string>("Yes", "random@getawaydriver", "gesture_nod_yes_hard"),
                new Tuple<string, string, string>("Bring it on", "gestures@m@standing@casual", "gesture_bring_it_on"),
                new Tuple<string, string, string>("Bye", "gestures@m@standing@casual", "gesture_bye_hard"),
                new Tuple<string, string, string>("Come Here", "gestures@m@standing@casual", "gesture_come_here_hard"),
                new Tuple<string, string, string>("Damn!", "gestures@m@standing@casual", "gesture_damn"),
                new Tuple<string, string, string>("Displeased", "gestures@m@standing@casual", "gesture_displeased"),
                new Tuple<string, string, string>("Easy now", "gestures@m@standing@casual", "gesture_easy_now"),
                new Tuple<string, string, string>("Hello", "gestures@m@standing@casual", "gesture_hello"),
                new Tuple<string, string, string>("I Will", "gestures@m@standing@casual", "gesture_i_will"),
                new Tuple<string, string, string>("Me", "gestures@m@standing@casual", "gesture_me_hard"),
                new Tuple<string, string, string>("No way!", "gestures@m@standing@casual", "gesture_no_way"),
                new Tuple<string, string, string>("Point", "gestures@m@standing@casual", "gesture_point"),
                new Tuple<string, string, string>("Shrug", "gestures@m@standing@casual", "gesture_shrug_hard"),
                new Tuple<string, string, string>("What?", "gestures@m@standing@casual", "gesture_what_hard"),
                new Tuple<string, string, string>("Why?", "gestures@m@standing@casual", "gesture_why"),
                new Tuple<string, string, string>("You", "gestures@m@standing@casual", "gesture_you_hard"),
                new Tuple<string, string, string>("It's mine!", "gestures@m@standing@casual", "getsure_its_mine"),
                new Tuple<string, string, string>("Shrug", "gestures@m@standing@casual", "gesture_shrug_hard"),
                new Tuple<string, string, string>("Shrug", "gestures@m@standing@casual", "gesture_shrug_hard"),
                new Tuple<string, string, string>("Shrug", "gestures@m@standing@casual", "gesture_shrug_hard"),
            }},
            {"Talking", new []
            {
                new Tuple<string, string, string>("Talk 1", "missfbi3_party_d", "stand_talk_loop_a_female"),
                new Tuple<string, string, string>("Talk 2", "missfbi3_party_d", "stand_talk_loop_a_male1"),
                new Tuple<string, string, string>("Talk 3", "missfbi3_party_d", "stand_talk_loop_a_male2"),
                new Tuple<string, string, string>("Talk 4", "missfbi3_party_d", "stand_talk_loop_a_male3"),
                new Tuple<string, string, string>("Talk 5", "missfbi3_party_d", "stand_talk_loop_b_male1"),
                new Tuple<string, string, string>("Talk 6", "missfbi3_party_d", "stand_talk_loop_b_male2"),
                new Tuple<string, string, string>("Talk 7", "missfbi3_party_d", "stand_talk_loop_b_male3"),
            }},
        };
    }
}