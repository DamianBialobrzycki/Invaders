using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Invaders.Model
{
    class InvadersModel
    {
        public readonly static Size PlayAreaSize = new Size(400, 300);
        public const int MaximumPlayerShots = 3;
        public const int InitialStarCount = 50;

        private readonly Random _random = new Random();

        public int Score { get; private set; }
        public int Wave { get; private set; }
        public int Lives { get; private set; }

        public bool GameOver { get; private set; }

        private DateTime? _playerDied = null;
        public bool PlayerDying { get { return _playerDied.HasValue; } }

        private Player _player;

        private readonly List<Invader> _invaders = new List<Invader>();
        private readonly List<Shot> _playerShots = new List<Shot>();
        private readonly List<Shot> _invadersShots = new List<Shot>();
        private readonly List<Point> _stars = new List<Point>();

        private Direction _invaderDirection = Direction.Left;
        private bool _justMovedDown = false;

        private DateTime _lastUpdated = DateTime.MinValue;

        public InvadersModel()
        {
            EndGame();
        }

        public void EndGame()
        {
            GameOver = true;
        }

        public void StartGame()
        {
            GameOver = false;

            foreach (Invader invader in _invaders)
                OnShipChanged(invader, true);
            _invaders.Clear();

            foreach (Shot shot in _playerShots)
                OnShotMoved(shot, true);
            _playerShots.Clear();
            _invadersShots.Clear();

            foreach (Point star in _stars)
                OnStarChanged(star, true);
            _stars.Clear();

            for (int i = 0; i < InitialStarCount; i++)
                AddStar();

            _player = new Player();
            OnShipChanged(_player, false);

            Lives = 2;
            Wave = 0;

            NextWave();
        }

        public void FireShot()
        {
            if (GameOver) return;

            var playerShots =
                from Shot shot in _playerShots
                where shot.Direction == Direction.Up
                select shot;

            if(playerShots.Count() < MaximumPlayerShots)
            {
                Point shotLocation = new Point(_player.Location.X + _player.Area.Width / 2, _player.Location.Y);
                Shot shot = new Shot(shotLocation, Direction.Up);
                _playerShots.Add(shot);
                OnShotMoved(shot, false);
            }
        }

        public void MovePlayer(Direction direction)
        {
            if (_playerDied.HasValue) return;
            _player.Move(direction);
            OnShipChanged(_player, false);
        }

        public void Twinkle()
        {
            if ((_random.Next(2) == 0) && _stars.Count > ((int)InitialStarCount * .75))
                RemoveAStar();
            else if (_stars.Count < ((int)InitialStarCount * 1.5))
                AddAStar();
        }

        private void RemoveAStar()
        {
            if (_stars.Count <= 0) return;
            int starIndex = _random.Next(_stars.Count);
            OnStarChanged(_stars[starIndex], true);
            _stars.RemoveAt(starIndex);
        }

        private void AddAStar()
        {
            Point point = new Point(_random.Next((int)PlayAreaSize.Width), _random.Next(20, (int)PlayAreaSize.Height) - 20);
            if (!_stars.Contains(point))
            {
                _stars.Add(point);
                OnStarChanged(point, false);
            }
        }

        public void Update(bool paused)
        {
            if(!paused)
            {
                if (_invaders.Count() == 0) NextWave();

                if(!_playerDied.HasValue)
                {
                    MoveInvaders();
                    MoveShots();
                    ReturnFire();
                    CheckForInvaderCollisions();
                    CheckForPlayerCollisions();
                }

            }
            else if(_playerDied.HasValue && (DateTime.Now - _playerDied > TimeSpan.FromSeconds(2.5)))
            {
                _playerDied = null;
                OnShipChanged(_player, false);
            }
            Twinkle();
        }

        private void MoveShots()
        {
            foreach (Shot shot in _playerShots)
            {
                shot.Move();
                OnShotMoved(shot, false);
            }

            var outOfBounds =
                from shot in _playerShots
                where (shot.Location.Y < 10 || shot.Location.Y > PlayAreaSize.Height - 10)
                select shot;

            foreach(Shot shot in outOfBounds.ToList())
            {
                _playerShots.Remove(shot);
                OnShotMoved(shot, true);
            }
        }
    }
}
