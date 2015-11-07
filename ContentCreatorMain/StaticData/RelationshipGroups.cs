using System.Collections.Generic;
using System.Runtime.InteropServices;
using Rage;

namespace MissionCreator.StaticData
{
    public static class RelationshipGroups
    {
        public static Dictionary<int, RelationshipGroup> Groups;

        public static void Init()
        {
            Groups = new Dictionary<int, RelationshipGroup>();


            {
                var group = new RelationshipGroup("MissionCreator_RESPECT");
                Game.SetRelationshipBetweenRelationshipGroups(Game.LocalPlayer.Character.RelationshipGroup, group, Relationship.Respect);
                Game.SetRelationshipBetweenRelationshipGroups(group, Game.LocalPlayer.Character.RelationshipGroup, Relationship.Respect);
                Groups.Add(1, group);
            }

            {
                var group = new RelationshipGroup("MissionCreator_LIKE");
                Game.SetRelationshipBetweenRelationshipGroups(Game.LocalPlayer.Character.RelationshipGroup, group, Relationship.Like);
                Game.SetRelationshipBetweenRelationshipGroups(group, Game.LocalPlayer.Character.RelationshipGroup, Relationship.Like);
                Groups.Add(2, group);
            }

            {
                var group = new RelationshipGroup("MissionCreator_NEUTRAL");
                Game.SetRelationshipBetweenRelationshipGroups(Game.LocalPlayer.Character.RelationshipGroup, group, Relationship.Neutral);
                Game.SetRelationshipBetweenRelationshipGroups(group, Game.LocalPlayer.Character.RelationshipGroup, Relationship.Neutral);
                Groups.Add(3, group);
            }

            {
                var group = new RelationshipGroup("MissionCreator_DISLIKE");
                Game.SetRelationshipBetweenRelationshipGroups(Game.LocalPlayer.Character.RelationshipGroup, group, Relationship.Dislike);
                Game.SetRelationshipBetweenRelationshipGroups(group, Game.LocalPlayer.Character.RelationshipGroup, Relationship.Dislike);
                Groups.Add(4, group);
            }

            {
                var group = new RelationshipGroup("MissionCreator_HATE");
                Game.SetRelationshipBetweenRelationshipGroups(Game.LocalPlayer.Character.RelationshipGroup, group, Relationship.Hate);
                Game.SetRelationshipBetweenRelationshipGroups(group, Game.LocalPlayer.Character.RelationshipGroup, Relationship.Hate);
                Groups.Add(5, group);
            }


            {
                RelationshipGroup group1;
                RelationshipGroup group2;
                group1 = new RelationshipGroup("MissionCreator_GROUP1");
                group2 = new RelationshipGroup("MissionCreator_GROUP2");
                Game.SetRelationshipBetweenRelationshipGroups(group2, group1, Relationship.Hate);
                Game.SetRelationshipBetweenRelationshipGroups(group1, group2, Relationship.Hate);
                Groups.Add(6, group1);
                Groups.Add(7, group2);
            }
        }
    }
}