using System;

namespace Invaders.Model
{
    internal class InvalidDirectionException : Exception
    {
        private Direction _direction;
        private Type _type;

        public InvalidDirectionException(Direction direction, Type type)
        {
            _direction = direction;
            _type = type;
        }
        
        public override string Message => $"Direction '{_direction}' is invalid for type '{_type}'.";
    }

    internal class InvalidInvaderTypeException : Exception
    {
        private int _wave;
        private int _row;
        private string _message;

        public InvalidInvaderTypeException(int wave, int row)
        {
            _wave = wave;
            _row = row;
        }

        public InvalidInvaderTypeException(string message)
        {
            _message = message;
        }

        public override string Message
        {
            get
            {
                if (_wave != null && _row !=null)
                {
                    _message = $"Failed to determine Invader type in wave {_wave} and row {_row}";
                }

                if (_message != null)
                {
                    return _message;
                }

                return "Invalid invader type or cannot determine invader type";
            }
        }
    }
}