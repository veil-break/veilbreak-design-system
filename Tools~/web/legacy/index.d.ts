import type * as React from 'react';

type Team = 'ally' | 'enemy';
type Cut = 'sm' | 'md' | 'lg' | '0';

/** Glyph names: ability shapes, roles, statuses, gear slots and HUD controls. */
export type IconName =
  | 'arrow' | 'fan' | 'pierce' | 'projectile' | 'cone' | 'burst' | 'zone' | 'beam' | 'dash' | 'wall'
  | 'ranged' | 'burst-role' | 'disruptor' | 'debuffer' | 'protector' | 'skirmisher' | 'support' | 'healer'
  | 'exposed' | 'silenced' | 'rooted' | 'slowed' | 'interrupted' | 'barrier'
  | 'optic' | 'core' | 'plating' | 'weapon' | 'charm'
  | 'queue' | 'menu' | 'signal' | 'settings' | 'surrender' | 'autopilot' | 'cancel' | 'check' | 'lock' | 'plus'
  | 'chevron-left' | 'chevron-right' | 'back' | 'play' | 'pause' | 'prev' | 'next' | 'replay' | 'team' | 'studio'
  | 'automation' | 'training' | 'key' | 'rift' | 'no-target' | 'warning' | 'diamond' | 'circle';

export interface IconProps { name: IconName; size?: number; title?: string; className?: string; style?: React.CSSProperties }
export declare function Icon(props: IconProps): React.ReactElement;

export interface PanelProps { variant?: 'hud' | 'raised' | 'solid' | 'ally' | 'enemy' | 'power'; cut?: Cut; title?: React.ReactNode; meta?: React.ReactNode; brackets?: boolean; scan?: boolean; desktop?: boolean; className?: string; style?: React.CSSProperties; children?: React.ReactNode }
export declare function Panel(props: PanelProps): React.ReactElement;

export interface HoloFrameProps { title?: React.ReactNode; tilt?: boolean; pose?: [number, number]; width?: number | string; cut?: Cut; animate?: boolean; className?: string; style?: React.CSSProperties; children?: React.ReactNode }
export declare function HoloFrame(props: HoloFrameProps): React.ReactElement;

export interface ButtonProps { variant?: 'cta' | 'light' | 'secondary' | 'ghost' | 'danger'; size?: 'md' | 'lg'; sub?: React.ReactNode; icon?: IconName; center?: boolean; blocked?: string; onClick?: () => void; className?: string; style?: React.CSSProperties; children?: React.ReactNode }
export declare function Button(props: ButtonProps): React.ReactElement;

export interface RailButtonProps { icon?: IconName; label?: string; ariaLabel?: string; active?: boolean; isNew?: boolean; desktop?: boolean; onClick?: () => void }
export declare function RailButton(props: RailButtonProps): React.ReactElement;

export interface SwitchProps { label?: React.ReactNode; sub?: React.ReactNode; checked?: boolean; onChange?: (next: boolean) => void }
export declare function Switch(props: SwitchProps): React.ReactElement;

export interface SegmentedOption { value?: string; label: string; sub?: string }
export interface SegmentedProps { options: SegmentedOption[]; value?: string; label?: string; onChange?: (value: string) => void; style?: React.CSSProperties }
export declare function Segmented(props: SegmentedProps): React.ReactElement;

export interface NavBarProps { active?: string; items?: string[]; rating?: number; onNavigate?: (item: string) => void }
export declare function NavBar(props: NavBarProps): React.ReactElement;

export interface MatchFighter { name: string; health?: number; team?: Team; defeated?: boolean }
export interface MatchHeaderProps { clock: string; fighters?: MatchFighter[]; phase?: 'normal' | 'pressure'; sub?: string; compact?: boolean }
export declare function MatchHeader(props: MatchHeaderProps): React.ReactElement;

export interface FighterPortraitProps {
  code: string; name?: string; health?: number; stability?: number; barrier?: number; team?: Team;
  state?: 'default' | 'selected' | 'exposed' | 'defeated'; auto?: boolean; queue?: number; slot?: number;
  variant?: 'rail' | 'card'; activity?: 'aiming' | 'idle' | 'auto' | 'moving' | 'casting' | string; onClick?: () => void;
}
export declare function FighterPortrait(props: FighterPortraitProps): React.ReactElement;

export interface AbilityButtonProps {
  label: string; icon?: IconName; cost?: number; hotkey?: string;
  state?: 'ready' | 'aiming' | 'queued' | 'windup' | 'cooldown' | 'short' | 'invalid' | 'silenced' | 'rejected';
  queueIndex?: number; time?: string; progress?: number; reason?: string; size?: 'md' | 'lg'; onClick?: () => void;
}
export declare function AbilityButton(props: AbilityButtonProps): React.ReactElement;

export interface QueueChipProps { index: number; skill: string; cost?: number; who?: string; eta?: string; next?: boolean; preview?: boolean; onCancel?: () => void }
export declare function QueueChip(props: QueueChipProps): React.ReactElement;

export interface PowerStripProps {
  value: number; max?: number; regen?: string; hint?: React.ReactNode; preview?: [number, number];
  marks?: { at: number; label?: string | number }[]; reserve?: number; queue?: Omit<QueueChipProps, 'index' | 'next'>[];
  legend?: boolean; desktop?: boolean; style?: React.CSSProperties;
}
export declare function PowerStrip(props: PowerStripProps): React.ReactElement;

export interface HealthBarProps { value: number; max?: number; stability?: number; barrier?: number; ghost?: number; team?: Team; exposed?: boolean; size?: 'world' | 'hud' | 'lg'; label?: string; style?: React.CSSProperties }
export declare function HealthBar(props: HealthBarProps): React.ReactElement;

export interface StatusChipProps { kind: 'exposed' | 'silenced' | 'rooted' | 'slowed' | 'interrupted' | 'barrier'; text?: string; detail?: string; time?: string }
export declare function StatusChip(props: StatusChipProps): React.ReactElement;

export interface ConnectionPillProps { ms?: number; region?: string; state?: 'reconnecting'; showState?: boolean }
export declare function ConnectionPill(props: ConnectionPillProps): React.ReactElement;

export interface ConnectionBannerProps { title?: string; children?: React.ReactNode }
export declare function ConnectionBanner(props: ConnectionBannerProps): React.ReactElement;

export interface FeedEntry { t: string; actor: string; team: Team | 'system'; text: string; target?: string; targetTeam?: Team; after?: string }
export interface CombatFeedProps { entries: FeedEntry[]; title?: string; style?: React.CSSProperties }
export declare function CombatFeed(props: CombatFeedProps): React.ReactElement;

export interface MinimapProps {
  fighters?: { x: number; y: number; team?: Team; selected?: boolean; defeated?: boolean }[];
  props?: { x: number; y: number; w: number; h: number; state?: 'intact' | 'cracked' | 'broken' | 'rubble' | 'anchor' }[];
  view?: [number, number, number, number]; safe?: [number, number, number, number]; width?: number; caption?: string | false; label?: string;
}
export declare function Minimap(props: MinimapProps): React.ReactElement;

export interface OffscreenIndicatorProps { name: string; team?: Team; side?: 'left' | 'right'; distance?: number; threat?: boolean }
export declare function OffscreenIndicator(props: OffscreenIndicatorProps): React.ReactElement;

export interface WorldLabelProps {
  kind?: 'aim' | 'name' | 'telegraph' | 'estimate' | 'blocked'; title?: React.ReactNode; cost?: number; detail?: string | string[];
  team?: Team; health?: number; stability?: number; barrier?: number; exposed?: boolean; stem?: boolean;
}
export declare function WorldLabel(props: WorldLabelProps): React.ReactElement;

export interface WorldMarkerProps { kind?: 'ally' | 'enemy' | 'selected' | 'queued' | 'move'; size?: number }
export declare function WorldMarker(props: WorldMarkerProps): React.ReactElement;

export interface TelegraphProps { shape: 'projectile' | 'fan' | 'cone' | 'burst' | 'zone' | 'beam' | 'dash' | 'wall'; team?: Team; state?: 'predicted' | 'confirmed'; progress?: number; width?: number; iso?: boolean }
export declare function Telegraph(props: TelegraphProps): React.ReactElement;

export interface CombatTextProps { value?: number | string; kind?: 'damage' | 'crit' | 'exposed' | 'taken' | 'heal' | 'barrier' | 'power' | 'miss'; animate?: boolean }
export declare function CombatText(props: CombatTextProps): React.ReactElement;

export interface HoloPlatformProps { label?: string; sub?: string; state?: 'occupied' | 'selected' | 'open'; team?: Team; marker?: boolean; onClick?: () => void }
export declare function HoloPlatform(props: HoloPlatformProps): React.ReactElement;

export interface TeamTileProps { code: string; name: string; role?: string; tier?: 'blue' | 'violet' | 'gold'; state?: 'available' | 'in' | 'selected' | 'locked'; onClick?: () => void }
export declare function TeamTile(props: TeamTileProps): React.ReactElement;

export interface ItemCellProps { slot?: 'optic' | 'core' | 'plating' | 'weapon' | 'charm'; rarity?: 'normal' | 'magic' | 'rare' | 'set' | 'unique'; upgrade?: number; state?: 'default' | 'selected' | 'empty' | 'locked'; showSlot?: boolean; name?: string; onClick?: () => void }
export declare function ItemCell(props: ItemCellProps): React.ReactElement;

export interface StatReadoutProps { label: string; value: number | string; unit?: string; delta?: string; deltaTone?: 'up' | 'down'; bar?: number; barTone?: 'ally' | 'power' | 'text' }
export declare function StatReadout(props: StatReadoutProps): React.ReactElement;

export interface RevealCardProps { tier?: 'blue' | 'violet' | 'gold'; name: string; code?: string; role?: string; kind?: 'fighter' | 'item'; slot?: ItemCellProps['slot']; flipped?: boolean; onFlip?: (next: boolean) => void; isNew?: boolean; dupe?: string; tierLabel?: string }
export declare function RevealCard(props: RevealCardProps): React.ReactElement;

export interface RuleRowProps {
  index: number; conditions: { text: string; value?: string; after?: string }[]; action: { title: string; rest?: string; cost?: string };
  join?: 'AND' | 'OR'; enabled?: boolean; note?: string; warning?: string; draft?: boolean;
}
export declare function RuleRow(props: RuleRowProps): React.ReactElement;

export interface TimelineProps { duration: number; position: number; events?: { t: number; team: Team | 'phase'; label?: string }[]; pressureFrom?: number; playing?: boolean; speed?: number }
export declare function Timeline(props: TimelineProps): React.ReactElement;

declare global {
  interface Window {
    Veilbreak: {
      Icon: typeof Icon; Panel: typeof Panel; HoloFrame: typeof HoloFrame; Button: typeof Button; RailButton: typeof RailButton;
      Switch: typeof Switch; Segmented: typeof Segmented; NavBar: typeof NavBar; MatchHeader: typeof MatchHeader;
      FighterPortrait: typeof FighterPortrait; AbilityButton: typeof AbilityButton; PowerStrip: typeof PowerStrip; QueueChip: typeof QueueChip;
      HealthBar: typeof HealthBar; StatusChip: typeof StatusChip; ConnectionPill: typeof ConnectionPill; ConnectionBanner: typeof ConnectionBanner;
      CombatFeed: typeof CombatFeed; Minimap: typeof Minimap; OffscreenIndicator: typeof OffscreenIndicator; WorldLabel: typeof WorldLabel;
      WorldMarker: typeof WorldMarker; Telegraph: typeof Telegraph; CombatText: typeof CombatText; HoloPlatform: typeof HoloPlatform;
      TeamTile: typeof TeamTile; ItemCell: typeof ItemCell; StatReadout: typeof StatReadout; RevealCard: typeof RevealCard;
      RuleRow: typeof RuleRow; Timeline: typeof Timeline;
    };
  }
}
