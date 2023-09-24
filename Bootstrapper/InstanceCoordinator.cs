using BloogBot.Models.Dto;
using System.Collections.Generic;

namespace Bootstrapper
{
    public static class InstanceCoordinator
    {
        public static readonly InstanceUpdate[] _allInstances = {
            _player01State, _player02State, _player03State, _player04State, _player05State,
            _player06State, _player07State, _player08State, _player09State, _player10State,
            _player11State, _player12State, _player13State, _player14State, _player15State,
            _player16State, _player17State, _player18State, _player19State, _player20State,
            _player21State, _player22State, _player23State, _player24State, _player25State,
            _player26State, _player27State, _player28State, _player29State, _player30State,
            _player31State, _player32State, _player33State, _player34State, _player35State,
            _player36State, _player37State, _player38State, _player39State, _player40State
        };

        public static bool ConsumeMessage(InstanceUpdate instanceUpdate)
        {
            for (int i = 0; i < 40; i++)
            {
                if (_allInstances[i] == null)
                {
                    for (int j = i; j < 40; j++)
                    {
                        if (_allInstances[j] != null)
                        {
                            _allInstances[i] = _allInstances[j];
                            _allInstances[j] = null;
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < 40; i++)
            {
                if (_allInstances[i] == null)
                {
                    _allInstances[i] = instanceUpdate;
                    break;
                }
                else if (_allInstances[i].ProcessId == instanceUpdate.ProcessId)
                {
                    _allInstances[i] = instanceUpdate;
                    break;
                }
            }
            return true;
        }

        public static bool RemoveInstanceByProcessId(int processId)
        {
            for (int i = 0; i < 40; i++)
            {
                if (_allInstances[i] != null && _allInstances[i].ProcessId == processId)
                {
                    _allInstances[i] = null;
                    return true;
                }
            }
            return false;
        }

        public static InstanceUpdate _player01State;
        public static InstanceUpdate _player02State;
        public static InstanceUpdate _player03State;
        public static InstanceUpdate _player04State;
        public static InstanceUpdate _player05State;

        public static InstanceUpdate _player06State;
        public static InstanceUpdate _player07State;
        public static InstanceUpdate _player08State;
        public static InstanceUpdate _player09State;
        public static InstanceUpdate _player10State;

        public static InstanceUpdate _player11State;
        public static InstanceUpdate _player12State;
        public static InstanceUpdate _player13State;
        public static InstanceUpdate _player14State;
        public static InstanceUpdate _player15State;

        public static InstanceUpdate _player16State;
        public static InstanceUpdate _player17State;
        public static InstanceUpdate _player18State;
        public static InstanceUpdate _player19State;
        public static InstanceUpdate _player20State;

        public static InstanceUpdate _player21State;
        public static InstanceUpdate _player22State;
        public static InstanceUpdate _player23State;
        public static InstanceUpdate _player24State;
        public static InstanceUpdate _player25State;

        public static InstanceUpdate _player26State;
        public static InstanceUpdate _player27State;
        public static InstanceUpdate _player28State;
        public static InstanceUpdate _player29State;
        public static InstanceUpdate _player30State;

        public static InstanceUpdate _player31State;
        public static InstanceUpdate _player32State;
        public static InstanceUpdate _player33State;
        public static InstanceUpdate _player34State;
        public static InstanceUpdate _player35State;

        public static InstanceUpdate _player36State;
        public static InstanceUpdate _player37State;
        public static InstanceUpdate _player38State;
        public static InstanceUpdate _player39State;
        public static InstanceUpdate _player40State;
    }
}
