import Logo from "@/components/Logo";
import Stepper from "@/components/Stepper";
import { IStoreState } from "@/interfaces/IStoreState";
import BasicInfo from "@/views/BasicInfo";
import Configurator from "@/views/Configurator";
import EndScreen from "@/views/EndScreen/index";
import GameOver from "@/views/GameOver";
import StartScreen from "@/views/Start";
import Stats from "@/views/Stats";
import React, { useEffect, useRef, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Dispatch } from "redux";
import "./App.css";
import { uiActions } from "./redux/ui";
import { Config } from "./utils/config";
import CharacterDisplay from "./views/Character";
import IdleScreen from "./views/IdleScreen";

const guidRegex = /^\/[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}$/i;

const App: React.FC = () => {
  const appRef: React.RefObject<HTMLDivElement> = useRef(null);
  const { showGameOver, isIdle, isCharacterDisplay } = useSelector((store: IStoreState) => store.ui);

  const [config] = useState(new Config());
  const dispatch: Dispatch = useDispatch();

  useEffect(() => {
    config.persistEventID();
    if (appRef.current) {
      const app = appRef.current;
      const { width, height } = app.getBoundingClientRect();

      const onResize = () => {
        const scale = Math.min(window.innerWidth / width, window.innerHeight / height);
        app.style.transform = `translate(-50%, -50%) scale(${scale})`;
      };

      window.addEventListener("resize", onResize);

      onResize();

      return () => window.removeEventListener("resize", onResize);
    }
  }, [appRef, config]);

  useEffect(() => {
    if (window.location.pathname === "/logged-in") {
      window.history.pushState(null, window.document.title, "/");
      dispatch(uiActions.setIdle(false));
      dispatch(uiActions.navigateTo(1));
    } else if (guidRegex.test(window.location.pathname)) {
      dispatch(uiActions.displayCharacter());
    }
  }, [dispatch]);

  const steps = [
    { name: "start", component: StartScreen },
    { name: "basic-info", component: BasicInfo },
    { name: "configure", component: Configurator },
    { name: "stats", component: Stats },
    { name: "end", component: EndScreen },
  ];

  return (
    <div className="app" ref={appRef}>
      {isIdle ? (
        <IdleScreen onExit={() => dispatch(uiActions.setIdle(false))} />
      ) : isCharacterDisplay ? (
        <>
          <CharacterDisplay />
          <Logo />
        </>
      ) : (
        <>
          <Stepper steps={steps} />
          {showGameOver && <GameOver />}
          <Logo />
          <span className="flash" />
        </>
      )}
    </div>
  );
};

export default App;
