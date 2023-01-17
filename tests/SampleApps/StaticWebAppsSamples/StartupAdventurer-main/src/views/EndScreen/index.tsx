import Character from "@/components/Character";
import StatGrader from "@/components/StatGrader";
import Floorlight from "@/graphics/floorlight";
import Lightbeam from "@/graphics/lightbeam";
import { IStoreState } from "@/interfaces/IStoreState";
import { uiActions } from "@/redux/ui";
import anime from "animejs";
import axios from "axios";
import domToImage from "dom-to-image";
import FileSaver from "file-saver";
import React, { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Dispatch } from "redux";
import StepperFooter from "~/src/components/Stepper/StepperFooter";
import formatCharacterData from "~/src/utils/format-character-data";
import ArrowDown from "./arrow-down";
import {
  CharacterArea,
  CharacterCard,
  CtaArea,
  EndButton,
  EndScreenContainer,
  EndScreenWrapper,
  PublicUrl,
  QRCodeContainer,
  Spinner,
  SpinnerContainer,
  Stats,
} from "./styles";

const EndScreen = () => {
  const [qrUrl, setQRUrl] = useState("");
  const [isFetching, setFetching] = useState(false);
  const { gradedStats } = useSelector((store: IStoreState) => store.stats);
  const store = useSelector((store: IStoreState) => store);
  const dispatch: Dispatch = useDispatch();

  const download = async () => {
    const characterWrapper = document.getElementById("character-card");

    if (!characterWrapper) {
      return;
    }

    const png = await domToImage.toPng(characterWrapper, { style: { transform: "none", opacity: 1 } });

    FileSaver.saveAs(png, "avatar.png");
  };

  const finish = () => {
    dispatch(uiActions.startOver());
  };

  useEffect(() => {
    let rendering = true;

    const createCharacterData = () => {
      const { ui, ...restStore } = store;

      return formatCharacterData(restStore);
    };

    const postCharacterToApi = async () => {
      const formData = createCharacterData();
      let token = "hello world";
      const response = await axios({
        method: "post",
        url: "/api/save-character",
        headers: {
          "Content-Type": "text/json",
          Authorization: `Bearer ${token}`,
        },
        data: JSON.stringify(formData),
      });

      const { data } = response;

      try {
        if (data && data.public_url) {
          setQRUrl(data.public_url);
        }
      } catch (e) {
        console.log(e);
      }
    };

    /* Do the magic */
    if (rendering && !qrUrl && !isFetching) {
      postCharacterToApi();
      setFetching(true);
    }

    return () => {
      rendering = false;
    };
  }, [qrUrl, isFetching, store]);

  useEffect(() => {
    anime.set("#character-area", { opacity: 0 });
    anime.set(".logo", { zIndex: -1 });
    anime.set("#character-card", {
      translateX: 750,
      translateY: -1000,
      scale: 1.2,
      zIndex: 99,
      opacity: 0,
    });
    anime.set("#character-area, .skill-container", { opacity: 1 });

    anime({
      targets: "span.flash",
      opacity: [1, 0],
      duration: 1000,
      easing: "linear",
    });

    anime({
      targets: ".view-quide-text",
      opacity: [0, 1],
      translateY: [30, 0],
      easing: "linear",
      duration: 500,
    });

    anime({
      targets: "#character-card",
      translateY: [-1000, -30],
      duration: 750,
      easing: "easeOutExpo",
      opacity: [0, 1],
      endDelay: 2000,

      complete: () => {
        anime({
          targets: "#character-card",
          duration: 750,
          easing: "easeOutExpo",

          complete: () => {
            anime.remove("#character-card");
            anime.set("#character-card", { translateY: -30, translateX: 750, scale: 1.2 });

            anime({
              targets: "#character-card",
              translateX: [750, 0],
              translateY: [-30, 0],
              scale: [1.2, 1],
              duration: 750,
              easing: "easeOutExpo",
            });

            anime({
              targets: ".dimmer",
              opacity: [1, 0],
              duration: 800,
              easing: "easeOutExpo",
              complete: () => {
                anime.set(".logo", { zIndex: 9 });
              },
            });
          },
        });
      },
    });
  }, []);

  return (
    <EndScreenWrapper>
      <p className="view-quide-text">The adventure continues!</p>
      <EndScreenContainer>
        <CharacterCard id="character-card">
          <CharacterArea id="character-area">
            <div className="lights">
              <Lightbeam className="lightbeam" />
              <Floorlight className="floorlight" />
            </div>
            <Character />
          </CharacterArea>
          <Stats>
            {gradedStats.map((stat, index: number) => (
              <StatGrader key={"statitem$$" + index} stat={stat} />
            ))}
          </Stats>
        </CharacterCard>
        <CtaArea>
          <h1>
            Mission accomplished
            <br />
            Startup Adventurer!
          </h1>
          <p className="cta-text">Scan QR code to get your avatar</p>
          <ArrowDown />
          {!qrUrl ? (
            <SpinnerContainer>
              <Spinner />
            </SpinnerContainer>
          ) : (
            <>
              <PublicUrl href={qrUrl} target="_blank">
                <QRCodeContainer>
                  <img src={`https://chart.googleapis.com/chart?chs=300x300&cht=qr&chl=${qrUrl}&choe=UTF-8`} alt="QR" />
                </QRCodeContainer>
              </PublicUrl>
            </>
          )}
          <EndButton onClick={download}>Download</EndButton>
          <EndButton onClick={finish}>Finish</EndButton>
        </CtaArea>
        <StepperFooter />
        <span className="dimmer" />
      </EndScreenContainer>
    </EndScreenWrapper>
  );
};

export default EndScreen;
