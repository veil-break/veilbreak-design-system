// @veilbreak/gui — generated from com.veilbreak.gui by Tools~/web/package.py. Do not edit.
import type * as React from 'react';

/* ── The kit (com.veilbreak.gui ported; render inside class="vb-root") ── */
export type KitIconName = string; // window.Veilbreak.VbIcon.names (kebab or PascalCase file name, or a design-system alias)
export type SkillState = 'ready' | 'aiming' | 'cooldown' | 'short' | 'silenced' | 'fallen' | 'casting' | 'disabled' | 'queued' | 'buffered' | 'windup' | 'invalid' | 'rejected' | 'exposed';
export type RarityTier = 'none' | 'blue' | 'violet' | 'gold';
export type WorldTeam = 'ally' | 'enemy';
export type ControlMode = 'auto' | 'manual' | 'aiming' | 'fallen';
export type PerkTier = 'general' | 'operative' | 'rare' | 'epic';
export type StatusKind = 'exposed' | 'silenced' | 'rooted' | 'slowed' | 'interrupted' | 'barrier';
export interface SkillView { label?: string; icon?: KitIconName; slot?: 'A' | 'S' | string; kind?: string; description?: string; fullDescription?: string; cost?: number; state?: SkillState; charged?: number; seconds?: number; cooldownSeconds?: number; need?: number; queue?: number; progress?: number; reason?: string }
export interface OperativeView { id?: string; name: string; code: string; role?: string; roleIcon?: KitIconName; passive?: string; passiveText?: string; passiveFullText?: string; kit: string; portrait?: string | null; hp: number; maxHp: number; barrier?: number; armor?: number; stability?: number; maxStability?: number; statuses?: StatusKind[]; mode?: ControlMode; manualSeconds?: number; skills?: SkillView[] }
export interface PerkView { id?: string; name: string; category?: string; tag?: string; icon?: KitIconName; effect?: string; now?: string; next?: string; tier?: PerkTier; art?: string | null; stack?: number; max?: number; waves?: number[] }
export interface QueueView { who: string; skill: string; cost: number; eta: string; actor?: number; slot?: number; manual?: boolean; next?: boolean }
export interface HudView { credits?: number; speed?: number; wave?: number; score?: number; boss?: boolean; auto?: boolean; slow?: boolean; power?: number; maxPower?: number; regen?: string; hint?: string | null; focus?: number; previewFrom?: number; previewTo?: number; queue?: QueueView[]; queueTitle?: string; queueHint?: string }
export interface BossView { name: string; health: number; stability: number; detail?: string; shielded?: boolean; interruptible?: boolean; shieldDetail?: string }
export interface ScoreView { rank: number; score: number; wave: number; bosses: number; squad: string[]; current?: boolean }
export interface ContributionView { operative: OperativeView; fellWave?: number; damage?: number; kills?: number; casts?: number; power?: number; share?: number; extraLabel?: string; extraValue?: string }

export interface VbFrameProps { kind?: 'slim' | 'chipped'; className?: string; style?: React.CSSProperties; children?: React.ReactNode }
export declare function VbFrame(props: VbFrameProps): React.ReactElement;
export interface VbIconProps { icon: KitIconName; size?: number; title?: string; className?: string; style?: React.CSSProperties }
export declare function VbIcon(props: VbIconProps): React.ReactElement;
export interface VbMarkProps { width?: number; className?: string; style?: React.CSSProperties }
export declare function VbMark(props: VbMarkProps): React.ReactElement;
export interface VbButtonProps { title: string; variant?: 'primary' | 'secondary' | 'light' | 'danger' | 'ghost'; sub?: string; icon?: KitIconName; blocked?: string; onClick?: () => void; className?: string; style?: React.CSSProperties }
export declare function VbButton(props: VbButtonProps): React.ReactElement;
export interface StatReadoutProps { label: string; value: string; unit?: string; bar?: number; delta?: string; up?: boolean; tone?: 'power' | 'text'; className?: string }
export declare function StatReadout(props: StatReadoutProps): React.ReactElement;
export interface StatusChipProps { kind: StatusKind; text?: string; detail?: string; time?: string; className?: string }
export declare function StatusChip(props: StatusChipProps): React.ReactElement;
export interface VbSwitchProps { text: string; detail?: string; value?: boolean; onChange?: (value: boolean) => void; className?: string }
export declare function VbSwitch(props: VbSwitchProps): React.ReactElement;
export interface VbSliderProps { label: string; value?: number; min?: number; max?: number; onChange?: (value: number) => void; className?: string }
export declare function VbSlider(props: VbSliderProps): React.ReactElement;
export interface VbSegmentedProps { items: { label: string; sub?: string }[]; selected?: number; onChange?: (index: number) => void; className?: string }
export declare function VbSegmented(props: VbSegmentedProps): React.ReactElement;
export interface VbMeterProps { value: number; extra?: number; previewFrom?: number; previewTo?: number; marks?: number[]; before?: React.ReactNode; after?: React.ReactNode; className?: string; style?: React.CSSProperties }
export declare function VbMeter(props: VbMeterProps): React.ReactElement;
export interface HealthBarProps { hp: number; max: number; barrier?: number; stability?: number; exposed?: boolean; enemy?: boolean; ghost?: number; size?: 'hud' | 'world' | 'large'; caption?: string; className?: string; style?: React.CSSProperties }
export declare function HealthBar(props: HealthBarProps): React.ReactElement;
export interface PowerStripProps { value: number; max?: number; regen?: string; hint?: string; previewFrom?: number; previewTo?: number; marks?: number[]; reserve?: number; queue?: QueueView[]; onCancel?: (entry: QueueView) => void; legend?: string; className?: string; style?: React.CSSProperties }
export declare function PowerStrip(props: PowerStripProps): React.ReactElement;
export interface FocusMeterProps { focus?: number; slow?: boolean; regenPerSecond?: number; drainPerSecond?: number; slowScale?: number; className?: string; style?: React.CSSProperties }
export declare function FocusMeter(props: FocusMeterProps): React.ReactElement;
export interface SkillButtonProps { view: SkillView; dragging?: boolean; flash?: boolean; onPointerDown?: (e: React.PointerEvent) => void; className?: string; style?: React.CSSProperties }
export declare function SkillButton(props: SkillButtonProps): React.ReactElement;
export interface RoadView { kind?: string; title: string; icon?: KitIconName; hint: string }
export interface RouteView { wave: number; left: RoadView; right: RoadView }
export interface MerchantOfferView { perk: PerkView; price: number; taken?: boolean; blocked?: string }
export interface MerchantStackView { perk: PerkView; payout: number }
export interface MerchantHealthView { actor: number; name: string; hp: number; maxHp: number; cost: number; payout: number; blocked?: string }
export interface MerchantView { credits: number; offers: MerchantOfferView[]; stacks: MerchantStackView[]; health: MerchantHealthView[] }
export declare function GlitchText(props: { text: string; effect?: 'in' | 'out' | 'slam'; className?: string; style?: React.CSSProperties }): React.ReactElement;
export declare function CreditsChip(props: { credits: number; className?: string }): React.ReactElement;
export declare function RouteChoice(props: { view: RouteView; onChoose?: (side: number) => void; className?: string; style?: React.CSSProperties }): React.ReactElement;
export declare function MerchantScreen(props: { view: MerchantView; onBuy?: (index: number) => void; onSellPerk?: (id: string) => void; onSellHealth?: (actor: number) => void; onLeave?: () => void; className?: string; style?: React.CSSProperties }): React.ReactElement;
export interface HudTopProps { view: HudView; showFocus?: boolean; queuePinned?: boolean; onAutoClick?: () => void; onSpeedClick?: () => void; onPauseClick?: () => void; onQueueCancel?: (entry: QueueView) => void; className?: string; style?: React.CSSProperties }
export declare function HudTop(props: HudTopProps): React.ReactElement;
export interface QueueListProps { queue: QueueView[]; title?: string; hint?: string; onCancel?: (entry: QueueView) => void; className?: string }
export declare function QueueList(props: QueueListProps): React.ReactElement;
export interface BossBarProps { view: BossView; className?: string }
export declare function BossBar(props: BossBarProps): React.ReactElement;
export interface ScreenHeaderProps { title: string; meta?: string; accent?: boolean; backLabel?: string; onBack?: () => void; className?: string }
export declare function ScreenHeader(props: ScreenHeaderProps): React.ReactElement;
export interface ScoreRowProps { view: ScoreView; className?: string }
export declare function ScoreRow(props: ScoreRowProps): React.ReactElement;
export interface OperativeCardProps { view: OperativeView; dragging?: number; onSkillPointerDown?: (index: number, e: React.PointerEvent) => void; className?: string; style?: React.CSSProperties }
export declare function OperativeCard(props: OperativeCardProps): React.ReactElement;
export interface OperativeTileProps { view: OperativeView; slot?: number; focused?: boolean; tier?: RarityTier; locked?: string; onPick?: () => void; className?: string }
export declare function OperativeTile(props: OperativeTileProps): React.ReactElement;
export interface SquadSlotProps { view: OperativeView | null; slot: number; onPick?: () => void; className?: string }
export declare function SquadSlot(props: SquadSlotProps): React.ReactElement;
export interface OperativeDetailsProps { view: OperativeView; slot?: number; squadFull?: boolean; maxHp?: number; maxArmor?: number; maxStability?: number; onClose?: () => void; onToggle?: () => void; className?: string; style?: React.CSSProperties }
export declare function OperativeDetails(props: OperativeDetailsProps): React.ReactElement;
export interface OperativeStatsProps { view: ContributionView; className?: string }
export declare function OperativeStats(props: OperativeStatsProps): React.ReactElement;
export interface SquadResultProps { view: ContributionView; pinned?: boolean; className?: string }
export declare function SquadResult(props: SquadResultProps): React.ReactElement;
export interface PerkCardProps { view: PerkView; selected?: boolean; onPick?: () => void; className?: string; style?: React.CSSProperties }
export declare function PerkCard(props: PerkCardProps): React.ReactElement;
export interface PerkDetailProps { view: PerkView; draft?: boolean; className?: string; style?: React.CSSProperties }
export declare function PerkDetail(props: PerkDetailProps): React.ReactElement;
export interface PerkChipProps { view: PerkView; alignEnd?: boolean; pinned?: boolean; className?: string }
export declare function PerkChip(props: PerkChipProps): React.ReactElement;
export interface PerkRowProps { view: PerkView; selected?: boolean; onPick?: () => void; className?: string }
export declare function PerkRow(props: PerkRowProps): React.ReactElement;
export interface QueueChipProps { view: QueueView; position?: number; preview?: boolean; onCancel?: (entry: QueueView) => void; className?: string; style?: React.CSSProperties }
export declare function QueueChip(props: QueueChipProps): React.ReactElement;
export interface DialogProps { title?: string; body?: string; bodyClass?: string; className?: string; style?: React.CSSProperties; children?: React.ReactNode }
export declare function Dialog(props: DialogProps): React.ReactElement;
export interface BackdropProps { image?: string; className?: string; style?: React.CSSProperties; children?: React.ReactNode }
export declare function Backdrop(props: BackdropProps): React.ReactElement;
export declare function ScanLines(props?: { className?: string }): React.ReactElement;
export interface HoloFrameProps { title?: string; still?: boolean; className?: string; style?: React.CSSProperties; children?: React.ReactNode }
export declare function HoloFrame(props: HoloFrameProps): React.ReactElement;
export interface HoloPlatformProps { name?: string; detail?: string; state?: 'occupied' | 'selected' | 'open'; team?: WorldTeam; showMarker?: boolean; onPick?: () => void; className?: string; style?: React.CSSProperties }
export declare function HoloPlatform(props: HoloPlatformProps): React.ReactElement;
export interface RevealCardProps { tier?: 'blue' | 'violet' | 'gold'; title: string; detail?: string; item?: boolean; isNew?: boolean; dupe?: string; flipped?: boolean; tierLabel?: string; onFlip?: () => void; className?: string; style?: React.CSSProperties }
export declare function RevealCard(props: RevealCardProps): React.ReactElement;
export interface RuleCondition { text: string; value?: string; after?: string }
export interface RuleView { index?: number; conditions: RuleCondition[]; any?: boolean; action: string; actionRest?: string; cost?: number; enabled?: boolean; warning?: boolean; draft?: boolean; note?: string }
export interface RuleRowProps { view: RuleView; onToggle?: (enabled: boolean) => void; className?: string; style?: React.CSSProperties }
export declare function RuleRow(props: RuleRowProps): React.ReactElement;
export interface WorldHealthBarProps { health?: number; barrier?: number; ghost?: number; team?: WorldTeam; rank?: 'normal' | 'elite' | 'boss'; tint?: string; className?: string; style?: React.CSSProperties }
export declare function WorldHealthBar(props: WorldHealthBarProps): React.ReactElement;
export type CombatTextKind = 'damage' | 'taken' | 'crit' | 'exposed' | 'heal' | 'barrier' | 'power' | 'miss' | 'execute' | 'break';
export interface CombatTextProps { kind?: CombatTextKind; amount?: number; className?: string; style?: React.CSSProperties }
export declare function CombatText(props: CombatTextProps): React.ReactElement;
export interface WorldLabelProps { kind?: 'aim' | 'name' | 'telegraph' | 'estimate' | 'blocked'; title: string; detail?: string; team?: WorldTeam; showDetail?: boolean; className?: string; style?: React.CSSProperties }
export declare function WorldLabel(props: WorldLabelProps): React.ReactElement;
export interface WorldMarkerProps { kind?: 'ally' | 'enemy' | 'selected' | 'queued' | 'move'; size?: number; className?: string; style?: React.CSSProperties }
export declare function WorldMarker(props: WorldMarkerProps): React.ReactElement;
export interface OffscreenIndicatorProps { name: string; team?: WorldTeam; metres?: number; threat?: boolean; angle?: number; className?: string; style?: React.CSSProperties }
export declare function OffscreenIndicator(props: OffscreenIndicatorProps): React.ReactElement;
export interface EdgeWarningProps { caption?: string; urgency?: number; top?: boolean; className?: string; style?: React.CSSProperties }
export declare function EdgeWarning(props: EdgeWarningProps): React.ReactElement;
/* ── 0.3.0: the handbook's atoms, composites, layouts and views ── */
type Base = { className?: string; style?: React.CSSProperties };
export type TypeStyle = 'display' | 'title' | 'label' | 'badge' | 'body' | 'caption' | 'num';
export type TextTone = 'default' | 'ally' | 'enemy' | 'power' | 'muted';
export declare function ProgressBar(props: Base & { segments?: number; value?: number; next?: boolean; tint?: string; tone?: 'ally' | 'power' | 'enemy' | 'neutral' }): React.ReactElement;
export declare function KeyValueRow(props: Base & { label: string; value: string; good?: boolean }): React.ReactElement;
export declare function ListRow(props: Base & { icon?: KitIconName; title: string; caption?: string; bar?: { segments?: number; value?: number; next?: boolean; tone?: string } | null; value?: string; action?: React.ReactNode; selected?: boolean; onPick?: () => void }): React.ReactElement;
export declare function TextComponent(props: Base & { text?: React.ReactNode; typeStyle?: TypeStyle; tone?: TextTone; children?: React.ReactNode }): React.ReactElement;
export declare function LabelComponent(props: Base & { text: string; size?: 'label' | 'badge'; tone?: TextTone }): React.ReactElement;
export type BadgeTone = 'neutral' | 'ally' | 'enemy' | 'power' | 'warn' | 'inverted';
export declare function BadgeComponent(props: Base & { text?: string; icon?: KitIconName; value?: string | number; tone?: BadgeTone; tooltip?: string; children?: React.ReactNode }): React.ReactElement;
export declare function IconButtonComposite(props: Base & { icon?: KitIconName; text?: string; tooltip: string; active?: boolean; quiet?: boolean; size?: number; textClass?: string; onClick?: () => void }): React.ReactElement;
export type TileState = 'in' | 'focused' | 'locked' | 'empty';
export declare function TileComponent(props: Base & { title?: string; caption?: string; captionIcon?: KitIconName; picture?: string | null; glyph?: KitIconName; kit?: string; order?: number; state?: TileState | TileState[]; tier?: RarityTier; emptyLabel?: string; tooltip?: string; block?: string; onPick?: () => void }): React.ReactElement;
export declare function CardComponent(props: Base & { title?: string; meta?: string; media?: React.ReactNode; actions?: React.ReactNode; selected?: boolean; children?: React.ReactNode }): React.ReactElement;
export declare function DrawerOverlay(props: Base & { open?: boolean; snap?: 'half' | 'full'; onSnap?: (snap: 'half' | 'full') => void; title?: string; meta?: string; label?: string; backdrop?: boolean; dismissible?: boolean; closeButton?: boolean; actions?: React.ReactNode; onDismiss?: () => void; children?: React.ReactNode }): React.ReactElement;
export declare function FieldComposite(props: Base & { label?: string; help?: string; children?: React.ReactNode }): React.ReactElement;
export declare function Fieldset(props: Base & { title: string; children?: React.ReactNode }): React.ReactElement;
export declare function FormComposite(props: Base & { saved?: string; headed?: boolean; note?: { icon?: KitIconName; text: string } | null; footer?: React.ReactNode; children?: React.ReactNode }): React.ReactElement;
export declare function StackLayout(props: Base & { gap?: 4 | 8 | 12 | 16; children?: React.ReactNode }): React.ReactElement;
export declare function VbGrid(props: Base & { columns?: 2 | 3 | 4; wrap?: boolean; children?: React.ReactNode }): React.ReactElement;
export declare function ListLayout(props: Base & { empty?: string; children?: React.ReactNode }): React.ReactElement;
export declare function BoardLayout(props: Base & { top?: React.ReactNode; world?: React.ReactNode; bottom?: React.ReactNode }): React.ReactElement;
export interface CheckpointView { wave: number; score: number; standing?: number; squadSize?: number; perks?: number; seed?: string }
export interface MainMenuViewModel { status?: string; subtitle?: string; notice?: string; best?: number; bestWave?: number; checkpoint?: CheckpointView | null; squad?: string[]; canExit?: boolean }
export declare function MainMenuView(props: Base & { view?: MainMenuViewModel; onPlay?: () => void; onResume?: () => void; onScores?: () => void; onSettings?: () => void; onExit?: () => void }): React.ReactElement;
export declare function MenuActionsSectionView(props: Base & { checkpoint?: CheckpointView | null; squadLine?: string; notice?: string; onPlay?: () => void; onResume?: () => void; onScores?: () => void; onSettings?: () => void }): React.ReactElement;
export interface SquadViewModel { roster: OperativeView[]; selected?: number[]; focus?: number; locked?: string[]; tiers?: RarityTier[]; seed?: string }
export declare function SquadView(props: Base & { view: SquadViewModel; onPick?: (index: number) => void; onSlotPick?: (slot: number) => void; onToggle?: (index: number) => void; onClose?: () => void; onStart?: () => void; onBack?: () => void }): React.ReactElement;
export interface SettingsViewModel { master?: number; music?: number; effects?: number; sensitivity?: number; haptics?: boolean; reducedMotion?: boolean; camera?: number; quality?: number; saved?: string; note?: string }
export declare function SettingsView(props: Base & { view?: SettingsViewModel; onChange?: (model: SettingsViewModel) => void; onClearScores?: () => void; onAccessibility?: () => void; onBack?: () => void }): React.ReactElement;
export interface PauseViewModel { wave: number; score: number; standing: number; squadSize?: number; perks?: PerkView[]; seed?: string; detail?: string }
export declare function PauseOverlayView(props: Base & { view: PauseViewModel; onResume?: () => void; onSettings?: () => void; onPerks?: () => void; onAbandon?: () => void; onExit?: () => void }): React.ReactElement;
export declare function ConfirmDialogView(props: Base & { title: string; body?: string; confirm?: { label: string; icon?: KitIconName; sub?: string; variant?: VbButtonProps['variant'] }; cancel?: { label: string; icon?: KitIconName }; onConfirm?: () => void; onCancel?: () => void }): React.ReactElement;
export declare function GuideDialogView(props: Base & { title?: string; steps?: string[]; onReady?: () => void }): React.ReactElement;
export interface SquadHealthView { code: string; hp: number; maxHp: number; stability?: number }
export interface PerkDraftViewModel { wave: number; bonus?: number; score?: number; choices: PerkView[]; pick?: number; squad?: SquadHealthView[] }
export declare function PerkDraftView(props: Base & { view: PerkDraftViewModel; onPick?: (index: number) => void; onTake?: () => void }): React.ReactElement;
export interface ResultsViewModel { cause?: string; score: number; wave: number; bosses?: number; rank?: number; of?: number; ledger?: { label: string; value: string }[]; distance?: string; simTime?: string; squad?: ContributionView[]; perks?: PerkView[] }
export declare function ResultsView(props: Base & { view: ResultsViewModel; onMenu?: () => void; onAgain?: () => void }): React.ReactElement;
export declare function HighScoresView(props: Base & { ranked: ScoreView[]; historical?: ScoreView[]; caption?: string; onBack?: () => void }): React.ReactElement;
export declare function RouteChoiceView(props: Base & { view: RouteView; onChoose?: (side: number) => void }): React.ReactElement;
export declare function MerchantScreenView(props: Base & { view: MerchantView; onBuy?: (index: number) => void; onSellPerk?: (id: string) => void; onSellHealth?: (actor: number) => void; onLeave?: () => void }): React.ReactElement;
export declare function BattleHudView(props: Base & { hud?: HudView; operatives?: OperativeView[]; boss?: BossView | null; announcement?: string; world?: React.ReactNode; queuePinned?: boolean; onPause?: () => void; onAuto?: () => void; onSpeed?: () => void; onQueueCancel?: (entry: QueueView) => void }): React.ReactElement;
export declare const Veilbreak: Record<string, unknown>;
export default Veilbreak;
