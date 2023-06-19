namespace Movement.Components
{
    public interface IDashReceiver : IRecevier
    {
        /*
        public enum Direction
        {
            No,
            Right,
            Left,
        }
        */
        public enum Stage
        {
            Dashed,
            Posible
        }

        public void Dash(Stage stage);
    }
}
