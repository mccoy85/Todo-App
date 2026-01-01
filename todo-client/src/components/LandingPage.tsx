import { Link } from 'react-router-dom';

// Simple landing view that routes users into the app.
export const LandingPage = () => {
  return (
    <div className="landing">
      <nav className="landing__nav">
        <div className="landing__brand">
          <img src="/tasky.svg" alt="Tasky logo" className="landing__badge" />
          Tasky
        </div>
      </nav>

      <main className="landing__hero">
        <div>
          <p className="landing__eyebrow">Simple, focused task tracking</p>
          <h1 className="landing__title">Tasky keeps your day on track.</h1>
          <p className="landing__subtitle">
            A lightweight space to capture tasks, set priorities, and check things off.
          </p>
          <div className="landing__cta">
            <Link to="/app" className="landing__button landing__button--primary">
              Launch Tasky
            </Link>
          </div>
        </div>
      </main>
    </div>
  );
};
