using System;
using System.Collections.Generic;
using System.Text;
using Firefly.Box;

namespace ENV.Security.Types
{
    class ID : ENV.Data.TextColumn
    {
        public ID(string Name)
            : base(Name, "32", Name)
        {
            
        }
        

        public ID()
            : this("ID")
        {
        }

        public void AddNewRowBehaviourTo(UIController uiController)
        {
            uiController.EnterRow +=
                delegate
                    {
                        if (uiController.Activity == Activities.Insert)
                            Common.SilentSet(this,Guid.NewGuid().ToString());
                    };
        }

        public void BindToParentID(ID id, UIController uiController)
        {
            uiController.Where.Add(IsEqualTo(id));
            uiController.EnterRow+=delegate()
                                       {
                                           if (uiController.Activity == Activities.Insert)
                                               Common.SilentSet(this, id);
                                       };
        }
        public void SetToNewValue()
        {
            Value = Guid.NewGuid().ToString();
        }

    }
}